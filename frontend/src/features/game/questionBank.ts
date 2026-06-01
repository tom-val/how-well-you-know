// Static, bilingual question suggestions — a concrete pool of ~1000 real questions.
//
// The pool is assembled once from a compact seed:
//   - every same-category "A or B?" pair (coherent: drinks vs drinks, cities vs cities…)
//   - a "Favourite X?" question per category
//   - a hand-written curated set of preference questions
// No cross-category pairs, so suggestions always make sense. All grammatically safe in
// Lithuanian and English.

export type Lang = "lt" | "en";

interface Bilingual {
  lt: string;
  en: string;
}

export interface Question {
  text: Bilingual;
  options: Bilingual[];
}

export interface Suggestion {
  text: string;
  options: string[];
}

interface Category {
  favourite: Bilingual; // "Favourite X?" phrasing (gendered correctly in Lithuanian)
  items: Bilingual[];
}

const CATEGORIES: Category[] = [
  {
    favourite: { en: "Favourite colour?", lt: "Mėgstamiausia spalva?" },
    items: [
      { en: "Red", lt: "Raudona" },
      { en: "Blue", lt: "Mėlyna" },
      { en: "Green", lt: "Žalia" },
      { en: "Yellow", lt: "Geltona" },
      { en: "Purple", lt: "Violetinė" },
      { en: "Orange", lt: "Oranžinė" },
      { en: "Pink", lt: "Rožinė" },
      { en: "Black", lt: "Juoda" },
      { en: "White", lt: "Balta" },
      { en: "Brown", lt: "Ruda" },
      { en: "Grey", lt: "Pilka" },
      { en: "Turquoise", lt: "Turkis" },
      { en: "Gold", lt: "Auksinė" },
      { en: "Silver", lt: "Sidabrinė" },
      { en: "Beige", lt: "Smėlinė" },
    ],
  },
  {
    favourite: { en: "Favourite food?", lt: "Mėgstamiausias maistas?" },
    items: [
      { en: "Pizza", lt: "Pica" },
      { en: "Sushi", lt: "Sušiai" },
      { en: "Burger", lt: "Mėsainis" },
      { en: "Pasta", lt: "Makaronai" },
      { en: "Tacos", lt: "Takai" },
      { en: "Salad", lt: "Salotos" },
      { en: "Steak", lt: "Kepsnys" },
      { en: "Soup", lt: "Sriuba" },
      { en: "Pancakes", lt: "Blynai" },
      { en: "Dumplings", lt: "Koldūnai" },
      { en: "Ice cream", lt: "Ledai" },
      { en: "Chocolate", lt: "Šokoladas" },
      { en: "Cheese", lt: "Sūris" },
      { en: "Curry", lt: "Karis" },
      { en: "Lasagne", lt: "Lazanija" },
    ],
  },
  {
    favourite: { en: "Favourite drink?", lt: "Mėgstamiausias gėrimas?" },
    items: [
      { en: "Coffee", lt: "Kava" },
      { en: "Tea", lt: "Arbata" },
      { en: "Beer", lt: "Alus" },
      { en: "Wine", lt: "Vynas" },
      { en: "Water", lt: "Vanduo" },
      { en: "Juice", lt: "Sultys" },
      { en: "Cola", lt: "Kola" },
      { en: "Lemonade", lt: "Limonadas" },
      { en: "Cocktail", lt: "Kokteilis" },
      { en: "Champagne", lt: "Šampanas" },
      { en: "Milk", lt: "Pienas" },
      { en: "Whiskey", lt: "Viskis" },
      { en: "Cider", lt: "Sidras" },
      { en: "Hot chocolate", lt: "Karštas šokoladas" },
      { en: "Kvass", lt: "Gira" },
    ],
  },
  {
    favourite: { en: "Favourite animal?", lt: "Mėgstamiausias gyvūnas?" },
    items: [
      { en: "Dog", lt: "Šuo" },
      { en: "Cat", lt: "Katė" },
      { en: "Horse", lt: "Arklys" },
      { en: "Rabbit", lt: "Triušis" },
      { en: "Lion", lt: "Liūtas" },
      { en: "Tiger", lt: "Tigras" },
      { en: "Elephant", lt: "Dramblys" },
      { en: "Dolphin", lt: "Delfinas" },
      { en: "Penguin", lt: "Pingvinas" },
      { en: "Owl", lt: "Pelėda" },
      { en: "Fox", lt: "Lapė" },
      { en: "Bear", lt: "Lokys" },
      { en: "Wolf", lt: "Vilkas" },
      { en: "Eagle", lt: "Erelis" },
      { en: "Panda", lt: "Panda" },
    ],
  },
  {
    favourite: { en: "Favourite hobby?", lt: "Mėgstamiausias pomėgis?" },
    items: [
      { en: "Reading", lt: "Skaitymas" },
      { en: "Gaming", lt: "Žaidimai" },
      { en: "Cooking", lt: "Gaminimas" },
      { en: "Painting", lt: "Tapyba" },
      { en: "Dancing", lt: "Šokiai" },
      { en: "Running", lt: "Bėgimas" },
      { en: "Fishing", lt: "Žvejyba" },
      { en: "Gardening", lt: "Sodininkystė" },
      { en: "Photography", lt: "Fotografija" },
      { en: "Hiking", lt: "Žygiai" },
      { en: "Singing", lt: "Dainavimas" },
      { en: "Cycling", lt: "Važinėjimas dviračiu" },
      { en: "Knitting", lt: "Mezgimas" },
      { en: "Chess", lt: "Šachmatai" },
      { en: "Travelling", lt: "Keliavimas" },
    ],
  },
  {
    favourite: { en: "Best place to visit?", lt: "Geriausia vieta aplankyti?" },
    items: [
      { en: "Beach", lt: "Paplūdimys" },
      { en: "Mountains", lt: "Kalnai" },
      { en: "City", lt: "Miestas" },
      { en: "Forest", lt: "Miškas" },
      { en: "Desert", lt: "Dykuma" },
      { en: "Island", lt: "Sala" },
      { en: "Countryside", lt: "Kaimas" },
      { en: "Lake", lt: "Ežeras" },
      { en: "River", lt: "Upė" },
      { en: "Jungle", lt: "Džiunglės" },
      { en: "Waterfall", lt: "Krioklys" },
      { en: "Volcano", lt: "Ugnikalnis" },
      { en: "Glacier", lt: "Ledynas" },
      { en: "Canyon", lt: "Kanjonas" },
      { en: "Cave", lt: "Urvas" },
    ],
  },
  {
    favourite: { en: "Dream city to visit?", lt: "Svajonių miestas?" },
    items: [
      { en: "Paris", lt: "Paryžius" },
      { en: "Tokyo", lt: "Tokijas" },
      { en: "Rome", lt: "Roma" },
      { en: "New York", lt: "Niujorkas" },
      { en: "London", lt: "Londonas" },
      { en: "Berlin", lt: "Berlynas" },
      { en: "Barcelona", lt: "Barselona" },
      { en: "Amsterdam", lt: "Amsterdamas" },
      { en: "Vilnius", lt: "Vilnius" },
      { en: "Prague", lt: "Praha" },
      { en: "Sydney", lt: "Sidnėjus" },
      { en: "Dubai", lt: "Dubajus" },
      { en: "Lisbon", lt: "Lisabona" },
      { en: "Vienna", lt: "Viena" },
      { en: "Istanbul", lt: "Stambulas" },
    ],
  },
  {
    favourite: { en: "Favourite movie genre?", lt: "Mėgstamiausias filmų žanras?" },
    items: [
      { en: "Comedy", lt: "Komedija" },
      { en: "Horror", lt: "Siaubas" },
      { en: "Action", lt: "Veiksmas" },
      { en: "Drama", lt: "Drama" },
      { en: "Romance", lt: "Romantika" },
      { en: "Thriller", lt: "Trileris" },
      { en: "Sci-fi", lt: "Mokslinė fantastika" },
      { en: "Fantasy", lt: "Fantastika" },
      { en: "Documentary", lt: "Dokumentika" },
      { en: "Animation", lt: "Animacija" },
      { en: "Crime", lt: "Kriminalas" },
      { en: "Adventure", lt: "Nuotykiai" },
    ],
  },
  {
    favourite: { en: "Favourite music genre?", lt: "Mėgstamiausias muzikos žanras?" },
    items: [
      { en: "Rock", lt: "Rokas" },
      { en: "Pop", lt: "Popsas" },
      { en: "Jazz", lt: "Džiazas" },
      { en: "Classical", lt: "Klasika" },
      { en: "Hip-hop", lt: "Hiphopas" },
      { en: "Electronic", lt: "Elektroninė" },
      { en: "Metal", lt: "Metalas" },
      { en: "Country", lt: "Kantri" },
      { en: "Reggae", lt: "Regis" },
      { en: "Folk", lt: "Folkloras" },
      { en: "Blues", lt: "Bliuzas" },
      { en: "Disco", lt: "Disko" },
    ],
  },
  {
    favourite: { en: "Favourite sport?", lt: "Mėgstamiausia sporto šaka?" },
    items: [
      { en: "Football", lt: "Futbolas" },
      { en: "Basketball", lt: "Krepšinis" },
      { en: "Tennis", lt: "Tenisas" },
      { en: "Swimming", lt: "Plaukimas" },
      { en: "Boxing", lt: "Boksas" },
      { en: "Skiing", lt: "Slidinėjimas" },
      { en: "Golf", lt: "Golfas" },
      { en: "Volleyball", lt: "Tinklinis" },
      { en: "Hockey", lt: "Ledo ritulys" },
      { en: "Cycling", lt: "Dviračių sportas" },
      { en: "Running", lt: "Bėgimas" },
      { en: "Rugby", lt: "Regbis" },
      { en: "Gymnastics", lt: "Gimnastika" },
      { en: "Climbing", lt: "Laipiojimas" },
      { en: "Karate", lt: "Karatė" },
    ],
  },
];

const CURATED: Question[] = [
  {
    text: { en: "Favourite season?", lt: "Mėgstamiausias metų laikas?" },
    options: [
      { en: "Spring", lt: "Pavasaris" },
      { en: "Summer", lt: "Vasara" },
      { en: "Autumn", lt: "Ruduo" },
      { en: "Winter", lt: "Žiema" },
    ],
  },
  {
    text: { en: "Ideal holiday?", lt: "Idealios atostogos?" },
    options: [
      { en: "Beach", lt: "Paplūdimys" },
      { en: "Mountains", lt: "Kalnai" },
      { en: "City break", lt: "Miestas" },
      { en: "Countryside", lt: "Kaimas" },
    ],
  },
  {
    text: { en: "Best meal of the day?", lt: "Geriausias dienos valgis?" },
    options: [
      { en: "Breakfast", lt: "Pusryčiai" },
      { en: "Lunch", lt: "Pietūs" },
      { en: "Dinner", lt: "Vakarienė" },
    ],
  },
  {
    text: { en: "Cats or dogs?", lt: "Katės ar šunys?" },
    options: [
      { en: "Cats", lt: "Katės" },
      { en: "Dogs", lt: "Šunys" },
    ],
  },
  {
    text: { en: "Tea or coffee?", lt: "Arbata ar kava?" },
    options: [
      { en: "Tea", lt: "Arbata" },
      { en: "Coffee", lt: "Kava" },
    ],
  },
  {
    text: { en: "Mountains or sea?", lt: "Kalnai ar jūra?" },
    options: [
      { en: "Mountains", lt: "Kalnai" },
      { en: "Sea", lt: "Jūra" },
    ],
  },
  {
    text: { en: "Sweet or savoury?", lt: "Saldu ar sūru?" },
    options: [
      { en: "Sweet", lt: "Saldu" },
      { en: "Savoury", lt: "Sūru" },
    ],
  },
  {
    text: { en: "Early bird or night owl?", lt: "Rytinis ar vakarinis žmogus?" },
    options: [
      { en: "Early bird", lt: "Rytinis" },
      { en: "Night owl", lt: "Vakarinis" },
    ],
  },
  {
    text: { en: "Books or movies?", lt: "Knygos ar filmai?" },
    options: [
      { en: "Books", lt: "Knygos" },
      { en: "Movies", lt: "Filmai" },
    ],
  },
  {
    text: { en: "Plan everything or improvise?", lt: "Viską planuoti ar improvizuoti?" },
    options: [
      { en: "Plan", lt: "Planuoti" },
      { en: "Improvise", lt: "Improvizuoti" },
    ],
  },
  {
    text: { en: "Dream superpower?", lt: "Svajonių supergalia?" },
    options: [
      { en: "Flight", lt: "Skraidymas" },
      { en: "Invisibility", lt: "Nematomumas" },
      { en: "Time travel", lt: "Laiko kelionės" },
      { en: "Telepathy", lt: "Telepatija" },
    ],
  },
  {
    text: { en: "Favourite holiday?", lt: "Mėgstamiausia šventė?" },
    options: [
      { en: "Christmas", lt: "Kalėdos" },
      { en: "New Year", lt: "Naujieji metai" },
      { en: "Birthday", lt: "Gimtadienis" },
      { en: "Easter", lt: "Velykos" },
    ],
  },
  {
    text: { en: "Text or call?", lt: "Žinutė ar skambutis?" },
    options: [
      { en: "Text", lt: "Žinutė" },
      { en: "Call", lt: "Skambutis" },
    ],
  },
  {
    text: { en: "How do you spend a free evening?", lt: "Kaip leidi laisvą vakarą?" },
    options: [
      { en: "At home", lt: "Namuose" },
      { en: "Out with friends", lt: "Su draugais" },
      { en: "At the cinema", lt: "Kine" },
      { en: "Doing a hobby", lt: "Su pomėgiu" },
    ],
  },
  {
    text: { en: "Favourite kind of weather?", lt: "Mėgstamiausias oras?" },
    options: [
      { en: "Sunny", lt: "Saulėta" },
      { en: "Rainy", lt: "Lietinga" },
      { en: "Snowy", lt: "Snieguota" },
      { en: "Cloudy", lt: "Debesuota" },
    ],
  },
  {
    text: { en: "Window or aisle seat?", lt: "Vieta prie lango ar prie tako?" },
    options: [
      { en: "Window", lt: "Prie lango" },
      { en: "Aisle", lt: "Prie tako" },
    ],
  },
  {
    text: { en: "City or countryside?", lt: "Miestas ar kaimas?" },
    options: [
      { en: "City", lt: "Miestas" },
      { en: "Countryside", lt: "Kaimas" },
    ],
  },
  {
    text: { en: "Save or spend?", lt: "Taupyti ar leisti?" },
    options: [
      { en: "Save", lt: "Taupyti" },
      { en: "Spend", lt: "Leisti" },
    ],
  },
  {
    text: { en: "Beach or pool?", lt: "Paplūdimys ar baseinas?" },
    options: [
      { en: "Beach", lt: "Paplūdimys" },
      { en: "Pool", lt: "Baseinas" },
    ],
  },
  {
    text: { en: "Comedy or horror?", lt: "Komedija ar siaubas?" },
    options: [
      { en: "Comedy", lt: "Komedija" },
      { en: "Horror", lt: "Siaubas" },
    ],
  },
  {
    text: { en: "Sweet or salty popcorn?", lt: "Saldūs ar sūrūs spragėsiai?" },
    options: [
      { en: "Sweet", lt: "Saldūs" },
      { en: "Salty", lt: "Sūrūs" },
    ],
  },
  {
    text: { en: "Shower in the morning or evening?", lt: "Dušas rytą ar vakarą?" },
    options: [
      { en: "Morning", lt: "Rytą" },
      { en: "Evening", lt: "Vakarą" },
    ],
  },
  {
    text: { en: "Pineapple on pizza?", lt: "Ananasai ant picos?" },
    options: [
      { en: "Yes", lt: "Taip" },
      { en: "No", lt: "Ne" },
    ],
  },
  {
    text: { en: "Window shopping or online shopping?", lt: "Apsipirkti parduotuvėje ar internetu?" },
    options: [
      { en: "In store", lt: "Parduotuvėje" },
      { en: "Online", lt: "Internetu" },
    ],
  },
  {
    text: { en: "Big party or small gathering?", lt: "Didelis vakarėlis ar mažas susibūrimas?" },
    options: [
      { en: "Big party", lt: "Didelis vakarėlis" },
      { en: "Small gathering", lt: "Mažas susibūrimas" },
    ],
  },
];

/** Builds the full question pool once: same-category pairs + favourites + curated. */
function buildPool(): Question[] {
  const pool: Question[] = [...CURATED];

  for (const category of CATEGORIES) {
    // "Favourite X?" with the first four items as options.
    pool.push({ text: category.favourite, options: category.items.slice(0, 4) });

    // Every coherent "A or B?" pair within the category.
    for (let i = 0; i < category.items.length; i++) {
      for (let j = i + 1; j < category.items.length; j++) {
        const a = category.items[i];
        const b = category.items[j];
        pool.push({
          text: {
            en: `${a.en} or ${b.en}?`,
            lt: `${a.lt} ar ${b.lt}?`,
          },
          options: [a, b],
        });
      }
    }
  }

  return pool;
}

const POOL = buildPool();

/** Number of distinct questions available (useful for sanity checks). */
export const QUESTION_COUNT = POOL.length;

/** Returns a random real question in the given language. */
export function suggestQuestion(lang: Lang): Suggestion {
  const q = POOL[Math.floor(Math.random() * POOL.length)];
  return { text: q.text[lang], options: q.options.map((o) => o[lang]) };
}
