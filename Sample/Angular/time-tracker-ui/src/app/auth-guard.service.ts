import { Injectable, Inject } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivateChild } from '@angular/router';
import { AdalGuard, AdalService } from 'adal-angular4';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';

@Injectable()
export class AuthGuardService implements CanActivate, CanActivateChild {

  constructor(
    private router: Router, 
    @Inject(LOCAL_STORAGE) private storage: StorageService,
    private _adalService: AdalService) {
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (this._adalService.userInfo.authenticated) {
      return true
    } else {
      return false;
    }
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this._adalService.userInfo.authenticated) {
      return true
    } else {
      return false;
    }
  } 
}