import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { NotFoundComponent } from './not-found/not-found.component';

import { RegisterComponent } from "./register/register.component";
import { AuthGuardService } from './auth-guard.service';
import { ReportsComponent } from './reports/reports.component';
import { KeyinComponent } from './keyin/keyin.component';

const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'home', component: HomeComponent },
    { path: 'reports', component: ReportsComponent, canActivate: [AuthGuardService] },
    { path: 'register', component: RegisterComponent },
    { path: 'keyin', component: KeyinComponent },
    { path: '**', component: NotFoundComponent }
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }