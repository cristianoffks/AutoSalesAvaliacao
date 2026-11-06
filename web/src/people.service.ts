import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface PersonInput {
  fullName: string;
  cpf: string;
  birthDate: string;
  email: string;
  phone: string;
}

export interface Person extends PersonInput {
  id: number;
}

@Injectable({ providedIn: 'root' })
export class PeopleService {
  private readonly http = inject(HttpClient);

  list(name: string, cpf: string) {
    let params = new HttpParams();

    if (name.trim()) {
      params = params.set('name', name.trim());
    }

    if (cpf.trim()) {
      params = params.set('cpf', cpf.trim());
    }

    return this.http.get<Person[]>('/api/people', { params });
  }

  get(id: number) {
    return this.http.get<Person>(`/api/people/${id}`);
  }

  delete(id: number) {
    return this.http.delete<void>(`/api/people/${id}`);
  }

  create(person: PersonInput) {
    return this.http.post<Person>('/api/people', person);
  }

  update(id: number, person: PersonInput) {
    return this.http.put<void>(`/api/people/${id}`, person);
  }
}
