import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class NotificationService {

    constructor(private toastr: ToastrService) { }

    success(message: string, title = 'Succès') {
        this.toastr.success(message, title);
    }

    warn(message: string, title = 'Attention') {
        this.toastr.warning(message, title);
    }

    error(message: string, title = 'Erreur') {
        this.toastr.error(message, title);
    }

    info(message: string, title = 'Information') {
        this.toastr.info(message, title);
    }
}
