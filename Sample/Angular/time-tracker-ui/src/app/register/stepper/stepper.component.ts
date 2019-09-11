import { Component, OnInit, forwardRef, Input, OnChanges } from '@angular/core';
import {
  ControlValueAccessor,
  FormControl,
  NG_VALUE_ACCESSOR
} from '@angular/forms';

@Component({
  selector: 'stepper-control',
  templateUrl: './stepper.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => StepperComponent),
      multi: true
    }
  ]
})
export class StepperComponent implements ControlValueAccessor {
  @Input('value') _value = 0;
  
  propagateChange: any = () => { };
  validateFn: any = () => { };


  constructor() { }

  get value() {
    return this._value;
  }

  set value(val) {
    this._value = val;
    this.propagateChange(val);
  }

  writeValue(value) {
    if (value) {
      this.value = value;
    }
  }

  registerOnChange(fn) {
    this.propagateChange = fn;
  }

  registerOnTouched() { }

  increment() {
    this.value++;
  }

  decrement() {
    this.value != 0 ? this.value-- : 0;
  }

  validate(c: FormControl) {
    return this.validateFn(c);
  }
}