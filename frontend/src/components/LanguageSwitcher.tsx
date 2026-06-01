import { useTranslation } from "react-i18next";

export function LanguageSwitcher() {
  const { i18n } = useTranslation();
  const current = i18n.resolvedLanguage ?? "lt";

  return (
    <div className="lang-switcher">
      {(["lt", "en"] as const).map((lng) => (
        <button
          key={lng}
          type="button"
          className={current === lng ? "active" : ""}
          onClick={() => i18n.changeLanguage(lng)}
        >
          {lng.toUpperCase()}
        </button>
      ))}
    </div>
  );
}
