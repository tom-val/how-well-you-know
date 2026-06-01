import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { STR } from "./strings";
import type { Lang, T } from "./strings";

interface LangContextValue {
  lang: Lang;
  setLang: (lang: Lang) => void;
  t: T;
}

const LangContext = createContext<LangContextValue | null>(null);

function readInitial(): Lang {
  const stored = localStorage.getItem("hwyk_lang");
  return stored === "en" || stored === "lt" ? stored : "lt";
}

export function LangProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>(readInitial);

  useEffect(() => {
    localStorage.setItem("hwyk_lang", lang);
    document.documentElement.lang = lang;
  }, [lang]);

  const setLang = useCallback((next: Lang) => setLangState(next), []);

  const value = useMemo<LangContextValue>(() => ({ lang, setLang, t: STR[lang] }), [lang, setLang]);

  return <LangContext.Provider value={value}>{children}</LangContext.Provider>;
}

export function useLang(): LangContextValue {
  const ctx = useContext(LangContext);
  if (!ctx) throw new Error("useLang must be used within a LangProvider");
  return ctx;
}

export function useT(): T {
  return useLang().t;
}
