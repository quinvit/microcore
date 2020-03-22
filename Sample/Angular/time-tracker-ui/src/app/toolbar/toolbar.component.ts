import { Component, OnInit, Inject } from '@angular/core';
import { AdalService } from 'adal-angular4';
import { Router } from '@angular/router';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';
import { NotificationService } from '../notification.service';
import { LayoutService } from '../layout.service';


@Component({
  selector: 'toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {
  constructor(
    private router: Router,
    @Inject(LOCAL_STORAGE) private storage: StorageService,
    private _notificationService: NotificationService,
    private _adalService: AdalService,
    private _layoutService: LayoutService) {
    }

  ngOnInit() {
    this._adalService.handleWindowCallback();
    // console.log(this._adalService.userInfo);
  }

  toggleSidenav(){
    if(this._layoutService.sidenavWidth == 0) {
      this._layoutService.increase();
    }
    else this._layoutService.decrease();
  }

  login() {
      this.storage.set('afterLoginUrl', '/reports');
      this._adalService.login();
  }

  logout() {
    this.storage.clear();
    this._adalService.logOut();
  }

  get authenticated(): boolean {
    var authenticated = this._adalService.userInfo.authenticated;
    if(authenticated != this._notificationService.authenticated){
      this._notificationService.loggedSubject.next(authenticated);
      this._notificationService.authenticated = authenticated;
    }
    return authenticated;
  }

  get userInfo(): any {
    return this._adalService.userInfo;
  }
}
