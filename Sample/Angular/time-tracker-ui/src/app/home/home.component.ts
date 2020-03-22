import { Component, OnInit, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';
import { AdalService } from 'adal-angular4';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  constructor(
    private router: Router,
    private _adalService: AdalService,
    @Inject(LOCAL_STORAGE) private storage: StorageService) {
  }

  get authenticated(): boolean {
    return this._adalService.userInfo.authenticated;
  }

  goTo(route: string) {
    this.router.navigate([route]);
  }

  ngOnInit() {
  }
}
