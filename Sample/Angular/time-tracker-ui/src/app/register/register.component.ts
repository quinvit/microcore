import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormArray, Validators } from "@angular/forms";
import { HttpClient, HttpBackend } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';

import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  register: FormGroup;
  http: HttpClient
  constructor(private _snackBar: MatSnackBar, private fb: FormBuilder, handler: HttpBackend) {
    this.http = new HttpClient(handler);
    this.initForm();
  }

  initForm() {
    this.register = this.fb.group({
      firstName: [''],
      lastName: [''],
      email: ['', [Validators.required, Validators.email, Validators.minLength(5)]],
      company: [''],
      yearsOfExperience: ['', Validators.maxLength(2)],
      jobTitle: [''],
      socialProfiles: this.fb.array([])
    })
  }

  get socialFields() {
    return <FormArray>this.register.get('socialProfiles');
  }

  getLinkControl() {
    return this.fb.group({
      'url': ['']
    })
  }

  addField() {
    const control = this.register.controls['socialProfiles'] as FormArray;
    control.push(this.getLinkControl());
  }

  onRegister() {
    var user = Object.assign({}, this.register.value);

    // Projection object[] to string[] by url property
    user.socialProfiles = user.socialProfiles.map((x: { url: string; }) => { return x.url; });

    var delay = 4000;
    this.http.post(`${environment.config.apiGateway}/api/users`, user)
      .subscribe(x => {
        this._snackBar.open(x === true ? 'Register success, please check your mailbox for further instruction' : 'Register failed, please try again with another email account.', 'OK', {
          duration: delay,
        });
      }, error => {
        this._snackBar.open('Register failed, please try again with another email account.', 'OK', {
          duration: delay,
        });
      });
  }

  ngOnInit() {
  }

}
