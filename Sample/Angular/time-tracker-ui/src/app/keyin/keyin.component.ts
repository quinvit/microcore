import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';

import { MatSnackBar } from '@angular/material/snack-bar';

import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { NotificationService } from '../notification.service';

export interface TimeRecord {
  description: string;
  totalMinutes: number;
  recordedTime: string;
  modifiedTime: string;
}


@Component({
  selector: 'keyin',
  templateUrl: './keyin.component.html',
  styleUrls: ['./keyin.component.scss']
})
export class KeyinComponent implements OnInit {
  keyin: FormGroup;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    private _notificationService: NotificationService) { 
      this.initForm();
    }

  ngOnInit() {
    this.initCurrentTimesheet();
  }

  initForm() {
    this.keyin = this.fb.group({
      username: [''],
      records: this.fb.array([])
    })
  }

  initCurrentTimesheet() {
    this.http.get<TimeRecord[]>(`${environment.config.apiGateway}/api/Reports/DailyReportByUser?date=${(new Date()).toISOString()}`)
      .subscribe(x => {
        this.initForm();
        this.addTimeRecords(x);
        this.addTimeRecords([{ totalMinutes: 15 } as TimeRecord]);
      });
  }

  onSubmit() {
    var records = this.keyin.value.records;
    var delay = 4000;
    this.http.post(`${environment.config.apiGateway}/api/Reports/Keyin`, records)
      .subscribe((x: any) => {
        this._snackBar.open(x.code === 0 ? 'Keyin success' : 'Keyin failed, please try again later', 'OK', {
          duration: delay,
        });

        (x.code === 0) && this.initCurrentTimesheet();
        this._notificationService.keyinSubject.next(x);

      }, error => {
        this._snackBar.open('Error, please try again later', 'OK', {
          duration: delay,
        });
      });
  }

  getTimeControl(record: TimeRecord) {
    return this.fb.group({
      'recordedTime': [record.recordedTime],
      'modifiedTime': [record.modifiedTime],
      'description': [record.description, Validators.maxLength(512)],
      'totalMinutes': [record.totalMinutes, Validators.maxLength(3)]
    })
  }

  addTimeRecords(records: TimeRecord[] = [{ totalMinutes: 15 } as TimeRecord]) {
    const control = this.keyin.controls['records'] as FormArray;
    records.forEach(record => {
      control.push(this.getTimeControl(record));
    });
  }

  get timeRecordFields() {
    return <FormArray>this.keyin.get('records');
  }
}
