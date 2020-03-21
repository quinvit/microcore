import { Component } from '@angular/core';
import { AdalService } from 'adal-angular4';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'TimeTracker';

  sidenavWidth = 4;
  increase(){
    this.sidenavWidth = 15;
  }

  decrease(){
    this.sidenavWidth = 4;
  }

  constructor(private adalService: AdalService) {
    adalService.init(environment.config);
  }

}
