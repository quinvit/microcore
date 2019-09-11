import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatRadioModule,
  MatSelectModule,
  MatSidenavModule,
  MatSlideToggleModule,
  MatTableModule,
  MatToolbarModule,
  MatTabsModule
} from '@angular/material';

const CommonModules = [
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatRadioModule,
  MatSelectModule,
  MatSidenavModule,
  MatSlideToggleModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule
];

@NgModule({
  imports: [
    CommonModule,
    ...CommonModules
  ],
  exports: [
    ...CommonModules
  ],
  declarations: []
})
export class MaterialModule { }
