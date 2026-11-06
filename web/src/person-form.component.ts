import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { FormsModule, NgForm, NgModel } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ErrorDialogComponent } from './error-dialog.component';
import { PeopleService, Person, PersonInput } from './people.service';
import { readApiError } from './api-error';

@Component({
  changeDetection: ChangeDetectionStrategy.Eager,
  selector: 'app-person-form',
  imports: [FormsModule, RouterLink, ErrorDialogComponent],
  templateUrl: './person-form.component.html',
})
export class PersonFormComponent implements OnInit {
  private readonly service = inject(PeopleService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  editingId: number | null = null;
  readonly today = new Date().toLocaleDateString('sv-SE');
  form: PersonInput | null = null;
  saving = false;
  error = '';
  success = '';
  loadFailed = false;
  invalidFields: string[] = [];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id === null) {
      this.form = { fullName: '', cpf: '', birthDate: '', email: '', phone: '' };
      return;
    }

    this.editingId = Number(id);
    if (!Number.isInteger(this.editingId) || this.editingId < 1) {
      this.loadFailed = true;
      this.error = 'Código de pessoa inválido.';
      return;
    }

    this.service.get(this.editingId).subscribe({
      next: ({ id, ...fields }) => (this.form = { ...fields, cpf: this.formatCpf(fields.cpf) }),
      error: response => {
        this.loadFailed = true;
        this.error = readApiError(response).message;
      },
    });
  }

  formatCpf(value: string): string {
    return value.replace(/\D/g, '').slice(0, 11)
      .replace(/^(\d{3})(\d)/, '$1.$2')
      .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
      .replace(/\.(\d{3})(\d)/, '.$1-$2');
  }

  updateCpf(value: string, field: NgModel): void {
    if (!this.form) {
      return;
    }

    this.form.cpf = this.formatCpf(value);
    if (value !== this.form.cpf) {
      field.control.setValue(this.form.cpf, { emitViewToModelChange: false });
    }
    this.clearFieldError('cpf');
  }

  formatPhone(value: string): string {
    const digits = value.replace(/\D/g, '').slice(0, 11);
    if (digits.length <= 2) {
      return digits;
    }

    const areaCode = digits.slice(0, 2);
    const number = digits.slice(2);
    const split = digits.length === 11 ? 5 : 4;
    if (number.length <= split) {
      return `(${areaCode}) ${number}`;
    }

    return `(${areaCode}) ${number.slice(0, split)}-${number.slice(split)}`;
  }

  birthDateInvalid(input: HTMLInputElement): boolean {
    return this.invalidFields.includes('birthdate') ||
      (input.value !== '' && !input.validity.valid);
  }

  updatePhone(value: string, field: NgModel): void {
    const person = this.form;
    if (!person) {
      return;
    }

    person.phone = this.formatPhone(value);
    if (value !== person.phone) {
      field.control.setValue(person.phone, { emitViewToModelChange: false });
    }
    this.clearFieldError('phone');
  }

  save(birthDateIsValid: boolean, personForm: NgForm): void {
    const person = this.form;
    if (!person || this.saving) {
      return;
    }

    if (!birthDateIsValid) {
      this.invalidFields = ['birthdate'];
      this.error = 'Informe uma data de nascimento válida e que não esteja no futuro.';
      return;
    }

    this.saving = true;
    this.error = '';
    this.success = '';
    this.invalidFields = [];
    const payload = { ...person, cpf: person.cpf.replace(/[.-]/g, '') };
    const request: Observable<Person | void> = this.editingId === null
      ? this.service.create(payload)
      : this.service.update(this.editingId, payload);

    request.subscribe({
      next: () => {
        this.saving = false;
        if (this.editingId === null) {
          this.form = { fullName: '', cpf: '', birthDate: '', email: '', phone: '' };
          personForm.resetForm({ ...this.form, personCpf: '' });
        }
        this.success = this.editingId === null
          ? 'Pessoa cadastrada com sucesso.'
          : 'Alterações salvas com sucesso.';
      },
      error: response => {
        this.saving = false;
        const error = readApiError(response);
        this.invalidFields = error.fields;
        this.error = error.message;
      },
    });
  }

  clearFieldError(field: string): void {
    this.invalidFields = this.invalidFields.filter(value => value !== field);
  }

  back(): void {
    void this.router.navigate(['/people']);
  }
}
