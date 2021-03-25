import { QuestionVariantDto } from "./questionVariant-dto.model";

export interface QuestionDto {
    id: string;
    name: string;
    multipleAnswers: boolean;
    variants: QuestionVariantDto[];
}