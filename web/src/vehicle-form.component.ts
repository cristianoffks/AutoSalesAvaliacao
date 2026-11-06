import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { FormsModule, NgModel } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { ErrorDialogComponent } from './error-dialog.component';
import { PeopleService, Person } from './people.service';
import { Vehicle, VehicleInput } from './vehicle';
import { VehicleService } from './vehicle.service';
import { readApiError } from './api-error';

@Component({
  changeDetection: ChangeDetectionStrategy.Eager,
  selector: 'app-vehicle-form',
  imports: [FormsModule, ErrorDialogComponent],
  templateUrl: './vehicle-form.component.html',
})
export class VehicleFormComponent implements OnInit {
  private readonly vehicleService = inject(VehicleService);
  private readonly peopleService = inject(PeopleService);
  private readonly route = inject(ActivatedRoute);

  editingId: number | null = null;
  plate = '';
  manufactureYear = '';
  brand = '';
  model = '';
  personId: number | null = null;
  ownerCpf = '';
  readonly currentYear = new Date().getFullYear();
  owner: Person | null = null;
  loadingOwner = false;
  ownerMessage = '';
  saving = false;
  error = '';
  invalidFields: string[] = [];
  submitted = false;
  saved = false;
  loadingVehicle = false;
  loadError = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id === null) {
      return;
    }

    this.editingId = Number(id);
    if (!Number.isInteger(this.editingId) || this.editingId < 1) {
      this.loadError = true;
      this.error = 'Código de veículo inválido.';
      return;
    }

    this.loadVehicle(this.editingId);
  }

  private loadVehicle(id: number, afterSave = false): void {
    this.loadingVehicle = true;
    this.vehicleService.get(id).subscribe({
      next: vehicle => {
        this.plate = vehicle.plate;
        this.manufactureYear = String(vehicle.manufactureYear);
        this.brand = vehicle.brand;
        this.model = vehicle.model;
        this.loadOwner(vehicle.personId);
        this.loadingVehicle = false;
      },
      error: response => {
        if (afterSave) {
          this.error = 'Alterações salvas, mas não foi possível recarregar o veículo.';
        } else {
          this.loadError = true;
          this.error = readApiError(response).message;
        }
        this.loadingVehicle = false;
      },
    });
  }

  private loadOwner(id: number): void {
    this.loadingOwner = true;
    this.peopleService.get(id).subscribe({
      next: person => {
        this.selectOwner(person);
        this.loadingOwner = false;
      },
      error: response => {
        this.showOwnerError(readApiError(response).message);
        this.loadingOwner = false;
      },
    });
  }

  clearOwner(): void {
    this.owner = null;
    this.personId = null;
    this.ownerMessage = '';
  }

  private selectOwner(person: Person): void {
    this.owner = person;
    this.personId = person.id;
    this.ownerCpf = person.cpf;
    this.clearFieldError('personid');
  }

  searchOwner(): void {
    if (this.loadingOwner || this.saving || this.editingId !== null) {
      return;
    }

    this.clearOwner();
    this.saved = false;
    const cpf = this.ownerCpf.trim().replace(/[.-]/g, '');
    if (!/^[0-9]{11}$/.test(cpf)) {
      this.showOwnerError('Informe um CPF com 11 dígitos.');
      return;
    }

    this.loadingOwner = true;
    this.peopleService.list('', cpf).subscribe({
      next: people => {
        const person = people[0] ?? null;
        if (person) {
          this.selectOwner(person);
        } else {
          this.showOwnerError('Nenhuma pessoa cadastrada com esse CPF.');
        }
        this.loadingOwner = false;
      },
      error: response => {
        this.showOwnerError(readApiError(response).message);
        this.loadingOwner = false;
      },
    });
  }

  save(formIsValid: boolean): void {
    if (this.saving || this.loadingOwner || this.invalidFields.length > 0) {
      return;
    }

    this.error = '';
    this.invalidFields = [];
    this.submitted = true;
    this.saved = false;

    if (this.personId === null) {
      this.error = 'Busque um proprietário pelo CPF antes de salvar.';
      return;
    }

    if (!formIsValid || !this.vehicleFieldsComplete || !this.validManufactureYear) {
      this.error = `Confira placa, marca, modelo e ano de fabricação (1000 a ${this.currentYear}).`;
      return;
    }

    this.saving = true;

    const vehicle = {
      plate: this.plate.trim(),
      manufactureYear: Number(this.manufactureYear),
      brand: this.brand.trim(),
      model: this.model.trim(),
      personId: this.personId,
    };
    this.saveRequest(vehicle).subscribe({
      next: () => {
        this.saving = false;
        this.submitted = false;
        if (this.editingId !== null) {
          this.saved = true;
          this.loadVehicle(this.editingId, true);
          return;
        }

        this.plate = '';
        this.manufactureYear = '';
        this.brand = '';
        this.model = '';
        this.ownerCpf = '';
        this.clearOwner();
        this.saved = true;
      },
      error: response => {
        const error = readApiError(response);
        const editableFields = ['plate', 'manufactureyear', 'brand', 'model'];
        if (this.editingId === null) {
          editableFields.push('personid');
        }

        this.invalidFields = error.fields.filter(field => editableFields.includes(field));
        this.error = error.message;
        this.saving = false;
      },
    });
  }

  private saveRequest(vehicle: VehicleInput): Observable<Vehicle | void> {
    return this.editingId === null
      ? this.vehicleService.create(vehicle)
      : this.vehicleService.update(this.editingId, vehicle);
  }

  get title(): string {
    return this.editingId === null ? 'Cadastrar veículo' : 'Editar veículo';
  }

  get successMessage(): string {
    return this.editingId === null ? 'Veículo cadastrado com sucesso.' : 'Alterações salvas com sucesso.';
  }

  get saveLabel(): string {
    return this.saving ? 'Salvando…' : 'Salvar';
  }

  isInvalid(field: string, invalid = false): boolean {
    return (this.submitted && invalid) || this.invalidFields.includes(field);
  }

  get validManufactureYear(): boolean {
    const year = Number(this.manufactureYear);
    return /^[0-9]{4}$/.test(this.manufactureYear) && year >= 1000 && year <= this.currentYear;
  }

  updateManufactureYear(value: string, field: NgModel): void {
    this.manufactureYear = value.replace(/\D/g, '').slice(0, 4);
    if (value !== this.manufactureYear) {
      field.control.setValue(this.manufactureYear, { emitViewToModelChange: false });
    }
    this.clearFieldError('manufactureyear');
  }

  private get vehicleFieldsComplete(): boolean {
    return !!(this.plate.trim() && this.brand.trim() && this.model.trim());
  }

  canSave(formIsValid: boolean): boolean {
    if (this.personId === null || this.saving || this.loadingOwner || this.invalidFields.length > 0) {
      return false;
    }


    return formIsValid && this.vehicleFieldsComplete && this.validManufactureYear;
  }

  clearFieldError(field: string): void {
    this.invalidFields = this.invalidFields.filter(value => value !== field);
  }

  private showOwnerError(message: string): void {
    this.ownerMessage = message;
    this.error = message;
  }
}

