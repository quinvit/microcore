import { Component, OnInit, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  constructor(
    private router: Router, 
    @Inject(LOCAL_STORAGE) private storage: StorageService) { 
  }

  ngOnInit() {
    if(this.storage.has("afterLoginUrl")) {
      this.router.navigateByUrl(this.storage.get("afterLoginUrl"));
    }
  }
}
