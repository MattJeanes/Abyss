import { Component } from '@angular/core';
import { TdDialogService } from '@covalent/core/dialogs';

import { GPTService } from './gpt.service';
import { IGPTMessage } from '../app.data';

@Component({
    templateUrl: './gpt.component.html',
    styleUrls: ['./gpt.component.scss'],
})
export class GPTComponent {
    public name = 'Someone';
    public log: IGPTMessage[] = [];
    public loading = false;
    public message = '';

    constructor(private gptService: GPTService, private dialogService: TdDialogService) { }

    public async generate(): Promise<void> {
        try {
            const currentText = this.log.map(x => x.text).join();
            if ((!currentText) || this.loading) { return; }
            this.loading = true;
            const response = await this.gptService.generate(currentText);
            response.text = this.removeLastLine(response.text).trimRight();
            this.log = [...this.log, response];
        } catch (e) {
            this.dialogService.openAlert({
                title: 'Failed to generate GPT text',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public add(message: string): void {
        this.log = [...this.log, { text: message, human: true }];
    }

    public clear(): void {
        this.log = [];
    }

    public undo(): void {
        this.log.pop();
    }

    private removeLastLine(input: string): string {
        if (input.lastIndexOf('\n') > 0) {
            return input.substring(0, input.lastIndexOf('\n'));
        } else {
            return input;
        }
    }
}
