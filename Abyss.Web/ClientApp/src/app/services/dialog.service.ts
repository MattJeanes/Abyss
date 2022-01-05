import { Injectable } from '@angular/core';

import { IDialogAlert, IDialogConfirm, IDialogPrompt } from '../app.data';

@Injectable()
export class DialogService {
    public async alert(alert: IDialogAlert): Promise<void> {
        // todo
    }

    public confirm(confirm: IDialogConfirm): Promise<boolean> {
        // todo
        return new Promise<boolean>((resolve, reject) => {
            resolve(false);
        });
    }

    public prompt(prompt: IDialogPrompt): Promise<string> {
        // todo
        return new Promise<string>((resolve, reject) => {
            resolve('');
        });
    }
}