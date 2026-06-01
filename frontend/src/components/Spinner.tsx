import { useTranslation } from "react-i18next";

export function Spinner() {
  const { t } = useTranslation();
  return (
    <div className="spinner" role="status" aria-live="polite">
      {t("common.loading")}
    </div>
  );
}
