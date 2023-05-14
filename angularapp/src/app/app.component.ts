import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AgGridAngular, AgGridModule } from 'ag-grid-angular';
import { ColDef } from 'ag-grid-community';
import { map, startWith, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatSelectModule,
    MatToolbarModule,
    MatTooltipModule,
    AgGridModule,
  ],
  template: `
    <div class="h-screen flex flex-col">
      <mat-toolbar color="primary">
        <div class="container mx-auto">
          <h1 class="text-xl">EF Core Playground</h1>
        </div>
      </mat-toolbar>
      <div class="container mx-auto my-4 grow flex flex-col">
        <mat-card appearance="outlined" class="p-4 mb-4">
          <form [formGroup]="formGroup" class="flex items-center">
            <mat-form-field class="me-2 -mb-6">
              <mat-label>Table</mat-label>
              <mat-select formControlName="model">
                <mat-option
                  *ngFor="let model of models$ | async"
                  [value]="model"
                >
                  {{ model.name }}
                </mat-option>
              </mat-select>
            </mat-form-field>
            <mat-checkbox formControlName="temporal"> Temporal </mat-checkbox>

            <div class="grow"></div>

            <button
              mat-icon-button
              aria-label="Add"
              matTooltip="Add"
              (click)="onAdd()"
            >
              <mat-icon fontSet="material-symbols-outlined">add</mat-icon>
            </button>
            <button
              mat-icon-button
              color="warn"
              aria-label="Remove"
              matTooltip="Remove"
              (click)="onRemove()"
            >
              <mat-icon fontSet="material-symbols-outlined">delete</mat-icon>
            </button>
            <button
              mat-flat-button
              color="primary"
              class="ms-2"
              (click)="onSave()"
            >
              Save
            </button>
          </form>
        </mat-card>

        <ag-grid-angular
          class="ag-theme-alpine grow"
          rowSelection="multiple"
          [columnDefs]="columnDefs$ | async"
          [defaultColDef]="defaultColDef"
          [rowData]="rowData$ | async"
        />
      </div>
    </div>
  `,
  styles: [],
})
export class AppComponent {
  http = inject(HttpClient);

  @ViewChild(AgGridAngular) agGrid!: AgGridAngular;

  formGroup = new FormGroup({
    model: new FormControl(),
    temporal: new FormControl(false),
  });

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
  };

  models$ = this.http
    .get<any[]>('/api/models')
    .pipe(tap((models) => this.formGroup.patchValue({ model: models[0] })));

  columnDefs$ = this.formGroup.valueChanges.pipe(
    map((value) =>
      value.model.properties.map((p: any, i: number) => ({
        field: `${p[0].toLowerCase()}${p.substring(1)}`,
        checkboxSelection: i === 0,
        headerCheckboxSelection: i === 0,
      }))
    )
  );

  rowData$ = this.formGroup.valueChanges.pipe(
    switchMap((value: any) =>
      this.http.get<any[]>(`/api/models/${value.model.name}`, {
        params: { temporal: value.temporal },
      })
    ),
    startWith([])
  );

  onAdd() {}

  onRemove() {}

  onSave() {}
}
