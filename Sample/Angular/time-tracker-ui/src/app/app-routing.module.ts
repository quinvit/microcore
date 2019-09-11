import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { NotFoundComponent } from './not-found/not-found.component';

import { LocationComponent } from "./location/location.component";
import { RegisterComponent } from "./register/register.component";
import { AuthGuardService } from './auth-guard.service';
import { ReportsComponent } from './reports/reports.component';

const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'reports', component: ReportsComponent, canActivate: [AuthGuardService] },
    { path: 'location', component: LocationComponent, canActivate: [AuthGuardService] },
    { path: 'register', component: RegisterComponent },
    { path: '**', component: NotFoundComponent }
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }