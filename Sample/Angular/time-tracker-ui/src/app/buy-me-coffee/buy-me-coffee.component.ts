import { Component, OnInit } from '@angular/core';
import { LayoutService } from '../layout.service';

@Component({
  selector: 'buy-me-coffee',
  templateUrl: './buy-me-coffee.component.html',
  styleUrls: ['./buy-me-coffee.component.css']
})
export class BuyMeCoffeeComponent implements OnInit {

  constructor(public _layoutService: LayoutService) { }

  ngOnInit() {
  }

}
