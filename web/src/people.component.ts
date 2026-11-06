import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { PeopleService, Person } from './people.service';
import { VehicleService } from './vehicle.service';
import { Vehicle } from './vehicle';
import { readApiError } from './api-error';

@Component({
  changeDetection: ChangeDetectionStrategy.Eager,
  selector: 'app-people',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './people.component.html',
})
export class PeopleComponent {
  private readonly service = inject(PeopleService);
  private readonly vehicleService = inject(VehicleService);
  name = '';
  cpf = '';
  groups: { person: Person; vehicles: Vehicle[] }[] = [];
  loading = false;
  deleting = false;
  error = '';
  search(): void {
    if (this.loading || this.deleting) {
      return;
    }
    this.loading = true;
    this.error = '';
    this.service.list(this.name, this.cpf).pipe(
      switchMap(people => this.vehicleService.listByOwners(people)),
      finalize(() => this.loading = false),
    ).subscribe({
      next: groups => this.groups = groups,
      error: response => this.error = readApiError(response).message,
    });
  }
  clear(): void {
    this.name = this.cpf = '';
    this.groups = [];
    this.error = '';
  }
  deletePerson(person: Person): void {
    if (this.deleting || !window.confirm(
      `Excluir ${person.fullName}?\n\nTodos os veículos vinculados a esta pessoa também serão excluídos. Esta ação não pode ser desfeita.`,
    )) return;
    this.deleting = true;
    this.error = '';
    this.service.delete(person.id).subscribe({
      next: () => { this.groups = this.groups.filter(group => group.person.id !== person.id); this.deleting = false; },
      error: response => { this.error = readApiError(response).message; this.deleting = false; },
    });
  }
}
