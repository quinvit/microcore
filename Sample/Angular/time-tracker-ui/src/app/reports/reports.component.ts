import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface DailyTimeSheetByUser {
  name: string;
  totalMinutes: number;
  dayInMonth: number;
  email: string;
}

const ELEMENT_DATA: DailyTimeSheetByUser[] = [
];

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {

  displayedColumns: string[] = ['name', 'totalMinutes', 'dayInMonth', "email"];
  dataSource = ELEMENT_DATA;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.http.get<DailyTimeSheetByUser[]>(`${environment.config.apiGateway}/api/Reports/MonthlyReportByUser`)
      .subscribe(x => {
        this.dataSource = x;
      });
  }
}
