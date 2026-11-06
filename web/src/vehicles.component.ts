import { HttpParams } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule, NgModel } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize, map, switchMap } from 'rxjs';
import { PeopleService, Person } from './people.service';
import { VehicleService } from './vehicle.service';
import { Vehicle } from './vehicle';
import { readApiError } from './api-error';
@Component({
  changeDetection: ChangeDetectionStrategy.Eager,
  selector: 'app-vehicles',
  imports: [FormsModule, RouterLink],
  templateUrl: './vehicles.component.html',
})
export class VehiclesComponent {
  private readonly peopleService = inject(PeopleService);
  private readonly vehicleService = inject(VehicleService);
  name = ''; cpf = ''; plate = ''; brand = ''; model = ''; year = '';
  rows: { vehicle: Vehicle; owner: Person }[] = [];
  loading = false; deleting = false; error = '';
  updateYear(value: string, field: NgModel): void {
    this.year = value.replace(/\D/g, '').slice(0, 4);
    if (value !== this.year) {
      field.control.setValue(this.year, { emitViewToModelChange: false });
    }
  }

  search(): void {
    if (this.loading || this.deleting) {
      return;
    }
    if (this.year && !/^[0-9]{4}$/.test(this.year)) {
      this.error = 'O ano de fabricação deve ter quatro dígitos.';
      return;
    }

    this.loading = true;
    this.error = '';
    const brand = this.brand.trim();
    const model = this.model.trim();
    const year = this.year.trim();
    let params = new HttpParams();
    if (this.plate.trim()) {
      params = params.set('plate', this.plate.trim());
    }
    if (brand) {
      params = params.set('brand', brand);
    }
    if (model) {
      params = params.set('model', model);
    }
    this.peopleService.list(this.name, this.cpf).pipe(
      switchMap(people => {
        return this.vehicleService.listByOwners(people, params);
      }),
      map(groups => groups.flatMap(({ person, vehicles }) => vehicles
        .filter(vehicle => !year || vehicle.manufactureYear === Number(year))
        .map(vehicle => ({ vehicle, owner: person })))),
      finalize(() => this.loading = false),
    ).subscribe({
      next: rows => this.rows = rows,
      error: response => this.error = readApiError(response).message,
    });
  }
  clear(): void {
    this.name = this.cpf = this.plate = this.brand = this.model = this.year = '';
    this.rows = [];
    this.error = '';
  }
  deleteVehicle(vehicle: Vehicle): void {
    if (this.deleting || !window.confirm(`Excluir o veículo ${vehicle.plate}?`)) return;
    this.deleting = true;
    this.error = '';
    this.vehicleService.delete(vehicle.id).subscribe({
      next: () => { this.rows = this.rows.filter(row => row.vehicle.id !== vehicle.id); this.deleting = false; },
      error: response => { this.error = readApiError(response).message; this.deleting = false; },
    });
  }
}
