import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

import { IDialogAlert, IDialogConfirm, IDialogPrompt } from '../app.data';

@Injectable()
export class DialogService {
    public openAlert(alert: IDialogAlert): OpenDialog<undefined> {
        // todo
        return new OpenDialog();
    }

    public openConfirm(confirm: IDialogConfirm): OpenDialog<boolean> {
        // todo
        return new OpenDialog();
    }

    public openPrompt(prompt: IDialogPrompt): OpenDialog<string> {
        // todo
        return new OpenDialog();
    }
}


class OpenDialog<T> {
    public afterClosed(): Subject<T> {
        // todo
        return new Subject<T>();
    }
}