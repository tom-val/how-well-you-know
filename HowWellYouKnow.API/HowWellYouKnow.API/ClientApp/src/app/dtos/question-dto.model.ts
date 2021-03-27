import { QuestionVariantDto } from "./questionVariant-dto.model";

export interface QuestionDto {
    id: string;
    name: string;
    order: number;
    multipleAnswers: boolean;
    variants: QuestionVariantDto[];
}