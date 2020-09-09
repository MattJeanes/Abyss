import { Component, OnInit } from '@angular/core';
import { TdDialogService } from '@covalent/core/dialogs';

import { GPTService } from './gpt.service';
import { IGPTMessage, IGPTModel } from '../app.data';

@Component({
    templateUrl: './gpt.component.html',
    styleUrls: ['./gpt.component.scss'],
})
export class GPTComponent implements OnInit {
    public name = 'Someone';
    public log: IGPTMessage[] = [];
    public loading = false;
    public message = '';
    public model?: string;
    public models: IGPTModel[] = [];

    constructor(private gptService: GPTService, private dialogService: TdDialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            this.loading = true;
            this.models = await this.gptService.getModels()

        } catch (e) {
            this.dialogService.openAlert({
                title: 'Failed to load GPT models',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public async generate(): Promise<void> {
        try {
            const currentText = this.log.map(x => x.Text).join();
            if ((!currentText) || (!this.model) || this.loading) { return; }
            this.loading = true;
            const response = await this.gptService.generate({ Text: currentText, ModelId: this.model });
            response.Text = this.removeLastLine(response.Text).trimRight();
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
        this.log = [...this.log, { Text: message, Human: true }];
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
