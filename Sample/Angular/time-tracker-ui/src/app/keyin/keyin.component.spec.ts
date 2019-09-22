import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KeyinComponent } from './keyin.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { NotificationService } from '../notification.service';
import { HttpClient, HttpHandler } from '@angular/common/http';
import { MatSnackBar } from '@angular/material';
import { Overlay } from '@angular/cdk/overlay';

describe('KeyinComponent', () => {
  let component: KeyinComponent;
  let fixture: ComponentFixture<KeyinComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KeyinComponent ],
      providers: [FormBuilder, HttpClient, HttpHandler, MatSnackBar, Overlay, NotificationService],
      schemas: [NO_ERRORS_SCHEMA]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KeyinComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
