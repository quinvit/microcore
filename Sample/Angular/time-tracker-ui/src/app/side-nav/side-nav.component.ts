import { Component } from '@angular/core';
import { Router } from "@angular/router";
import { AdalService } from 'adal-angular4';
import { NotificationService } from '../notification.service';
import { LayoutService } from '../layout.service';

@Component({
    selector: 'nav-items',
    template: `
        <mat-nav-list>
            <mat-list-item *ngFor="let item of items" (click)="goTo(item.route)">
                <mat-icon matListIcon>{{item.icon}}</mat-icon>
                <h3 matLine>{{item.title}}</h3>
            </mat-list-item>
        </mat-nav-list>`
})

export class SideNavComponent {
    items = [];

    constructor(
      private router: Router,
      private adalService: AdalService,
      private _notificationService: NotificationService,
      private _layoutService: LayoutService) {
        this.render();
        this._notificationService.loggedSubject.subscribe(x => this.render());
    }

    render(){
        this.items = [
            {
                title: 'Timesheet',
                icon: 'assignment',
                route: 'reports',
                requiredAuthenticated: true
            },
            {
                title: 'Sign up',
                icon: 'person_add',
                route: 'register',
                requiredAuthenticated: false
            },
            {
                title: 'About me',
                icon: 'contact_support',
                route: 'home',
                requiredAuthenticated: true
            }
        ].filter(x => x.requiredAuthenticated == this.authenticated);
    }

    get authenticated(): boolean {
        return this.adalService.userInfo.authenticated;
    }

    goTo(route: string) {
      this._layoutService.decrease();
        this.router.navigate([route]);
    }

}
