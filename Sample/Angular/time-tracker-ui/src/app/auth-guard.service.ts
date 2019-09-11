import { Injectable, Inject } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AdalGuard, AdalService } from 'adal-angular4';
import { LOCAL_STORAGE, StorageService } from 'ngx-webstorage-service';

@Injectable()
export class AuthGuardService extends AdalGuard {
  _adalService: AdalService;

  constructor(
    private router: Router, 
    @Inject(LOCAL_STORAGE) private storage: StorageService,
    adalService: AdalService) {
    super(adalService);
    this._adalService = adalService;
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    if (this._adalService.userInfo.authenticated) {
      return true
    } else {
      this.storage.set('afterLoginUrl', state.url);
      this._adalService.login();
      return false;
    }
  }
}