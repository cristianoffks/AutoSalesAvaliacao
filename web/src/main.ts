import { provideHttpClient } from '@angular/common/http';
import { provideZoneChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';
import { PeopleComponent } from './people.component';
import { PersonFormComponent } from './person-form.component';
import { VehicleFormComponent } from './vehicle-form.component';
import { VehiclesComponent } from './vehicles.component';

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection(),
    provideHttpClient(),
    provideRouter([
      { path: '', redirectTo: 'vehicles', pathMatch: 'full' },
      { path: 'people', component: PeopleComponent },
      { path: 'people/new', component: PersonFormComponent },
      { path: 'people/:id/edit', component: PersonFormComponent },
      { path: 'vehicles/new', component: VehicleFormComponent },
      { path: 'vehicles/:id/edit', component: VehicleFormComponent },
      { path: 'vehicles/by-owner', redirectTo: 'vehicles', pathMatch: 'full' },
      { path: 'vehicles', component: VehiclesComponent },
    ]),
  ],
}).catch(error => console.error(error));
