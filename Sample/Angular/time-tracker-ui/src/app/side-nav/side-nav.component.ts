import { Component } from '@angular/core';
import { Router } from "@angular/router";
import { AdalService } from 'adal-angular4';
import { NotificationService } from '../notification.service';

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

    constructor(private router: Router, private adalService: AdalService, private _notificationService: NotificationService) {
        this.render();
        this._notificationService.loggedSubject.subscribe(x => this.render());
    }

    render(){
        this.items = [
            {
                title: 'Timesheet',
                icon: 'history',
                route: 'reports',
                requiredAuthenticated: true
            },
            {
                title: 'Register',
                icon: 'person_add',
                route: 'register',
                requiredAuthenticated: false
            },
            {
                title: 'About me',
                icon: 'sentiment_satisfied_alt',
                route: 'home',
                requiredAuthenticated: true
            }
        ].filter(x => x.requiredAuthenticated == this.authenticated);
    }

    get authenticated(): boolean {
        return this.adalService.userInfo.authenticated;
    }

    goTo(route: string) {
        this.router.navigate([route]);
    }

}
