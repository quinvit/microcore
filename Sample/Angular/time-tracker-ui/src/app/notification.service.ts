import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  authenticated: boolean = false;
  readonly keyinSubject: Subject<any>;
  readonly loggedSubject: Subject<any>;

  constructor() {
    this.keyinSubject = new Subject<any>();
    this.loggedSubject = new Subject<any>();
   }
}
