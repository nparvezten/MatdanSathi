export type SupportedLocale = 'en' | 'mr' | 'hi';

export interface PublicUtilitiesTranslations {
  decoderTitle: string;
  decoderDesc: string;
  timeMachineTitle: string;
  timeMachineDesc: string;
  transliteratorTitle: string;
  transliteratorDesc: string;
  langSelectLabel: string;
}

export const TRANSLATIONS: Record<SupportedLocale, PublicUtilitiesTranslations> = {
  en: {
    decoderTitle: '🔍 Legacy EPIC Card Decoder',
    decoderDesc: 'Old Voter IDs do not use the modern alphanumeric format. Type legacy IDs to identify their district boundaries.',
    timeMachineTitle: '⏳ Delimitation Time Machine (1951 - 2026)',
    timeMachineDesc: 'Check how assembly boundaries shifted across pre-1976, 2008 delimitation, and upcoming ward redistricting drives.',
    transliteratorTitle: '🗣 Phonetic Marathi Name Transliterator',
    transliteratorDesc: 'Type voter names in English to preview standardized Devanagari Marathi spelling for Form 8 correction submissions.',
    langSelectLabel: 'Language / भाषा'
  },
  mr: {
    decoderTitle: '🔍 जुने मतदार ओळखपत्र डीकोडर',
    decoderDesc: 'जुन्या मतदार ओळखपत्रांमध्ये आधुनिक अक्षरांकीय फॉरमॅट वापरला जात नाही. जिल्हा सीमा ओळखण्यासाठी जुने आयडी टाइप करा.',
    timeMachineTitle: '⏳ मतदारसंघ पुनर्रचना टाईम मशीन (१९५१ - २०२६)',
    timeMachineDesc: '१९७६ पूर्वीच्या, २००८ मधील आणि आगामी पुनर्रचना मोहिमांमध्ये विधानसभेच्या सीमा कशा बदलल्या ते तपासा.',
    transliteratorTitle: '🗣 इंग्रजी ते मराठी नाव रूपांतरण',
    transliteratorDesc: 'फॉर्म ८ दुरुस्ती अर्जांसाठी देवनागरी मराठी स्पेलिंगचे पूर्वावलोकन करण्यासाठी इंग्रजीमध्ये नावे टाइप करा.',
    langSelectLabel: 'भाषा / Language'
  },
  hi: {
    decoderTitle: '🔍 पुराना मतदाता पहचान पत्र डिकोडर',
    decoderDesc: 'पुराने वोटर आईडी में आधुनिक अल्फान्यूमेरिक फॉर्मेट का उपयोग नहीं होता है। जिला सीमाओं की पहचान करने के लिए पुराने आईडी टाइप करें।',
    timeMachineTitle: '⏳ परिसीमन टाइम मशीन (1951 - 2026)',
    timeMachineDesc: 'जांचें कि 1976 से पहले, 2008 के परिसीमन और आगामी वार्ड पुनर्निर्धारण अभियानों में विधानसभा की सीमाएं कैसे बदलीं।',
    transliteratorTitle: '🗣 ध्वन्यात्मक मराठी नाम लिप्यंतरण',
    transliteratorDesc: 'फॉर्म 8 सुधार आवेदनों के लिए मानक देवनागरी मराठी वर्तनी का पूर्वावलोकन करने के लिए अंग्रेजी में नाम टाइप करें।',
    langSelectLabel: 'भाषा / Language'
  }
};
