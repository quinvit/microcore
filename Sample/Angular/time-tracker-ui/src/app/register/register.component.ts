import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormArray, Validators } from "@angular/forms";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  designations: string[];
  events: string[];
  register: FormGroup;

  constructor(private fb: FormBuilder) {
    this.initForm();
  }

  initForm() {
    this.register = this.fb.group({
      first: ['', [Validators.required, Validators.pattern('[a-zA-Z]+')]],
      last: [''],
      email: ['', [Validators.required, Validators.email, Validators.minLength(5)]],
      company: [''],
      experience: ['', Validators.maxLength(2)],
      track: [''],
      website: [''],
      social: this.fb.array([]),
      designation: [''],
      guest: [''],
      events: this.fb.array([])
    })
  }

  onRegister() {  
    console.log("Form Value", this.register.value);
  }

  get socialFileds() {
    return <FormArray>this.register.get('social');
  }

  getLinkControl() {
    return this.fb.group({
      'url': ['']
    })
  }

  addField() {
    const control = this.register.controls['social'] as FormArray;
    control.push(this.getLinkControl());
  }

  ngOnInit() {
    this.designations = [
      'Student/Trainee engineer',
      'Software Engineer (Web)',
      'Software Engineer (Mobile)',
      'Entrepreneur',
      'Other Profession'
    ];
    // this.events = [
    //   'DevFest Ahmedabad 2016',
    //   'DevFest Ahmedabad 2015',
    //   'DevFest Ahmedabad 2014',
    //   'DevFest Ahmedabad 2013',
    //   'Women Techmakers: Ahmedabad',
    //   'Google I/O Extended Ahmedabad',
    //   'None'
    // ]
  }

}
