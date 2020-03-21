import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {

  sidenavWidth = 0;
  increase(){
    this.sidenavWidth = 15;
  }

  decrease(){
    this.sidenavWidth = 0;
  }

  constructor() { }
}
