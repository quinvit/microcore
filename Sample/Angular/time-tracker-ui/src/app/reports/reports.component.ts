import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { NotificationService } from '../notification.service';
import { LayoutService } from '../layout.service';

export interface DailyTimeSheetByUser {
  description: string;
  tasks: string[];
  totalMinutes: number;
  dayInMonth: number;
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {

  thisMonthVisible: boolean = false;

  displayedColumns: string[] = ['description', 'totalMinutes', 'dayInMonth'];
  dataSource = [];

  constructor(
    private http: HttpClient,
    private _layoutService: LayoutService,
    private _notificationService: NotificationService) { }

  toggleThisMonth(){
    this.thisMonthVisible = !this.thisMonthVisible;
    this._layoutService.scrollToSection(this.thisMonthVisible ? 'thisMonthSection' : 'todayWorkSection');
  }

  ngOnInit() {
    this.query();
    this._notificationService.keyinSubject.subscribe(x => {
      this.query();
    });
  }

  query(){
    this.http.get<DailyTimeSheetByUser[]>(`${environment.config.apiGateway}/api/Reports/MonthlyReportByUser`)
      .subscribe(x => {
        this.dataSource = x.map(r => {
          r.tasks = r.description.split(' | ');
          return r;
        });
      });
  }
}
