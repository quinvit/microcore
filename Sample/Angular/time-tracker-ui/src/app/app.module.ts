import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { StorageServiceModule } from 'ngx-webstorage-service';

import { AdalService, AdalGuard, AdalInterceptor } from 'adal-angular4';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { FlexLayoutModule } from "@angular/flex-layout";
import { MaterialModule } from "./material.module";
import { RouterModule } from '@angular/router';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';

import { AuthGuardService } from './auth-guard.service';

import { HomeComponent } from './home/home.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { SideNavComponent } from "./side-nav/side-nav.component";
import { LocationComponent } from "./location/location.component";
import { RegisterComponent } from './register/register.component';
import { StepperComponent } from './register/stepper/stepper.component';
import { ReportsComponent } from './reports/reports.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NotFoundComponent,
    LocationComponent,
    ReportsComponent,
    SideNavComponent,
    ToolbarComponent,
    RegisterComponent,
    StepperComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FlexLayoutModule,
    AppRoutingModule,
    RouterModule,
    MaterialModule,
    FormsModule,
    ReactiveFormsModule,
    StorageServiceModule,
    HttpClientModule
  ],
  providers: [
    AdalService,
    AuthGuardService,
    AdalGuard,
    { provide: HTTP_INTERCEPTORS, useClass: AdalInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
