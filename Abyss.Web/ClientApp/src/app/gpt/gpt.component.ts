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
    public get model(): string | undefined {
        const data = localStorage['GPT.Model'];
        return data ? JSON.parse(data) : undefined;
    }
    public set model(value: string | undefined) {
        localStorage['GPT.Model'] = JSON.stringify(value);
    }
    public get removeIncompleteLine(): boolean {
        const data = localStorage['GPT.RemoveIncompleteLine'];
        return data ? JSON.parse(data) : true;
    }
    public set removeIncompleteLine(value: boolean) {
        localStorage['GPT.RemoveIncompleteLine'] = JSON.stringify(value);
    }
    public message = '';
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
            if ((!this.model) || this.loading) { return; }
            this.loading = true;
            const response = await this.gptService.generate({ Text: currentText, ModelId: this.model });
            if (this.removeIncompleteLine) {
                response.Text = this.removeLastLine(response.Text).trimRight();
            }
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
        if (!message) { return; }
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
