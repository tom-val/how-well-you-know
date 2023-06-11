import { HttpClient } from '@angular/common/http';
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Inject,
  OnInit,
  QueryList,
  ViewChildren,
} from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuestionDto } from '../dtos/question-dto.model';

@Component({
  selector: 'create-question-dialog',
  templateUrl: './create-question-dialog.component.html',
  styleUrls: ['./create-question-dialog.component.css'],
})
export class CreateQuestionDialogComponent implements OnInit, AfterViewInit {
  @ViewChildren('input') inputs: QueryList<ElementRef>;

  form: FormGroup;
  gameId: string;

  notation = 'A';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private changeDetectorRef: ChangeDetectorRef,
    @Inject('BASE_URL') private baseUrl: string,
    private _snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<CreateQuestionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data
  ) {
    this.gameId = data.gameId;
  }
  ngAfterViewInit(): void {
    this.inputs.changes.subscribe(() => {
      this.inputs.last.nativeElement.focus();
    });
  }

  ngOnInit() {
    this.form = this.fb.group({
      name: new FormControl('', [
        Validators.required,
        Validators.maxLength(200),
      ]),
      multipleAnswers: new FormControl(false, [Validators.required]),
      variants: this.fb.array([this.createVariant()]),
    });
  }

  nextChar(c) {
    return String.fromCharCode(c.charCodeAt(0) + 1);
  }

  createVariant(): FormGroup {
    const group = this.fb.group({
      name: new FormControl('', [Validators.required]),
      notation: this.notation,
    });
    this.notation = this.nextChar(this.notation);
    return group;
  }

  addVariant() {
    const items = this.form.get('variants') as FormArray;
    items.push(this.createVariant());
  }

  deleteValue(index: number) {
    const items = this.form.get('variants') as FormArray;
    items.removeAt(index);
  }

  save() {
    if (
      !this.form.valid ||
      (this.form.get('variants') as FormArray).length < 2
    ) {
      return;
    }

    const question = this.form.value as QuestionDto;

    this.http
      .post<string>(
        this.baseUrl + 'api/game/' + this.gameId + '/question',
        question
      )
      .subscribe(
        (result) => {
          console.log(result);
        },
        (error) => this._snackBar.open(JSON.stringify(error.message))
      );

    this.dialogRef.close(question);
  }

  keyPress(event: KeyboardEvent) {
    if (event.code === 'Enter') {
      this.addVariant();
      this.changeDetectorRef.detectChanges();
      this.inputs.last.nativeElement.focus();
    }
  }

  close() {
    this.dialogRef.close();
  }
}
