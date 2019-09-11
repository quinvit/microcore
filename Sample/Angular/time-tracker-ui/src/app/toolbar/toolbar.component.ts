import { Component, OnInit, Inject } from '@angular/core';
import { AdalService } from 'adal-angular4';
import { Router } from '@angular/router';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';

@Component({
  selector: 'toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {
  constructor(
    private router: Router, 
    @Inject(LOCAL_STORAGE) private storage: StorageService,
    private adalService: AdalService) { }

  ngOnInit() {
    this.adalService.handleWindowCallback();
    console.log(this.adalService.userInfo);
  }

  login() {
    this.router.navigateByUrl("/reports");
  }

  logout() {
    this.storage.clear();
    this.adalService.logOut();
  }

  get authenticated(): boolean {
    return this.adalService.userInfo.authenticated;
  }

  get userInfo(): any {
    return this.adalService.userInfo;
  }
}
