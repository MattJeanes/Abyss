import { Component } from '@angular/core';

import { IWhoSaid } from '../app.data';
import { WhoSaidService } from './whosaid.service';

import { DialogService } from '../services';

@Component({
    templateUrl: './whosaid.component.html',
    styleUrls: ['./whosaid.component.scss'],
})
export class WhoSaidComponent {
    public name = 'Someone';
    public log: IWhoSaid[] = [];
    public loading = false;
    public message = '';

    constructor(private whoSaidService: WhoSaidService, private dialogService: DialogService) { }

    public async whoSaid(): Promise<void> {
        try {
            if ((!this.message) || this.loading) { return; }
            this.loading = true;
            const whoSaid = await this.whoSaidService.whoSaid(this.message);
            this.log = [...this.log, whoSaid];
            this.name = whoSaid.Name;
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to get who said it',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public clear(): void {
        this.log = [];
    }

    public undo(): void {
        this.log.pop();
    }
}
