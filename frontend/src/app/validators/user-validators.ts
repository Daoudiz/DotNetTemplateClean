import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const MustMatch =
    (controlName: string, matchingControlName: string): ValidatorFn =>
        (group: AbstractControl): ValidationErrors | null => {

            const control = group.get(controlName);
            const matchingControl = group.get(matchingControlName);

            if (!control || !matchingControl) return null;

            const errors = matchingControl.errors || {};

            if (control.value !== matchingControl.value) {
                if (!errors['mustMatch']) {
                    matchingControl.setErrors({ ...errors, mustMatch: true });
                }
                return { mustMatch: true };
            } else {
                if (errors['mustMatch']) {
                    delete errors['mustMatch'];
                    matchingControl.setErrors(
                        Object.keys(errors).length ? errors : null
                    );
                }
                return null;
            }
        };
export default MustMatch; 