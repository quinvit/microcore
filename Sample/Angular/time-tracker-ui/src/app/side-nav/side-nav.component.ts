import { Component } from '@angular/core';
import { Router } from "@angular/router";
import { AdalService } from 'adal-angular4';

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

    constructor(private router: Router, private adalService: AdalService) {
        this.items = [
            {
                title: 'Timesheet',
                icon: 'bar_chart',
                route: 'reports'
            },
            {
                title: 'Location',
                icon: 'location_city',
                route: 'location'
            },
            {
                title: 'Register',
                icon: 'person_add',
                route: 'register'
            }
        ];
    }

    get authenticated(): boolean {
        return this.adalService.userInfo.authenticated;
    }

    goTo(route: string) {
        this.router.navigate([route]);
    }
}