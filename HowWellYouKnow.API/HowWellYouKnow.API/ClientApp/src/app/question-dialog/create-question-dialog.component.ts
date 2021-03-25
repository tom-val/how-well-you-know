
import { HttpClient } from "@angular/common/http";
import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { QuestionDto } from "../dtos/question-dto.model";
import { QuestionVariantDto } from "../dtos/questionVariant-dto.model";

@Component({
    selector: 'create-question-dialog',
    templateUrl: './create-question-dialog.component.html',
    styleUrls: ['./create-question-dialog.component.css']
})
export class CreateQuestionDialogComponent implements OnInit {

    form: FormGroup;
    gameId:string;

    notation = 'A';
    variants = new  Array<QuestionVariantDto>(); 

    constructor(
        private fb: FormBuilder,
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        private _snackBar: MatSnackBar,
        private dialogRef: MatDialogRef<CreateQuestionDialogComponent>,
        @Inject(MAT_DIALOG_DATA) data) {

        this.gameId = data.gameId;
    }

    ngOnInit() {
        this.form = this.fb.group({
            name: new FormControl('', [Validators.required, Validators.maxLength(200)]),
            multipleAnswers: new FormControl(false, [Validators.required]),
            variant: new FormControl('', [Validators.maxLength(200)])
        });
    }

    nextChar(c) {
        return String.fromCharCode(c.charCodeAt(0) + 1);
    }
    
    addVariant() {
        const formControl = this.form.controls['variant'];
        if (formControl.valid) {
            this.variants.push({
                name: formControl.value,
                notation: this.notation
            });
            this.notation = this.nextChar(this.notation);
            formControl.setValue('');
        }
    }

    save() {
        const question = this.form.value as QuestionDto;
        question.variants = this.variants;

        this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/question', question).subscribe(result => {
            console.log(result);
          }, error => this._snackBar.open(JSON.stringify(error.message)));

        this.dialogRef.close(question);
    }

    close() {
        this.dialogRef.close();
    }
}