import _ from 'lodash'
import Promise from 'bluebird'
import fs from 'fs-extra'

import nodeFetch from './node-fetch-wrapper.js'

const voices = [
    "Adagio Dazzle",
    // "Administrator",
    // "Alphys",
    // "Announcer",
    // "Apple Bloom",
    // "Applejack",
    // "Aria Blaze",
    // "Asgore",
    // "Asriel",
    // "Badeline",
    // "Big Mac",
    // "Black Mesa Announcer",
    // "Bon Bon",
    // "Braeburn",
    // "Carl Brutananadilewski",
    // "Cheerilee",
    // "Chell",
    // "Coco Pommel",
    // "Cozy Glow",
    // "Dan",
    // "Daria Morgendorffer",
    // "Daring Do",
    // "Demoman",
    // "Derpy Hooves",
    // "Diamond Tiara",
    // "Discord",
    // "Engineer",
    // "Flowey",
    // "Fluttershy",
    // "Gabby",
    // "Gaster",
    // "Gilda",
    // "GLaDOS",
    // "Gordon Freeman",
    // "Granny",
    // "HAL 9000",
    // "Heavy",
    // "Jane Lane",
    // "Kyu Sugardust",
    // "Lancer",
    // "Lightning Dust",
    // "Limestone Pie",
    // "Lyra",
    // "Madeline",
    // "Maud Pie",
    // "Medic",
    // "Mettaton",
    // "Minuette",
    // "Miss Pauling",
    // "Moondancer",
    // "Octavia",
    // "Oshiro",
    // "Overwatch",
    // "Papyrus",
    // "Pinkie Pie",
    // "Princess Cadance",
    // "Princess Celestia",
    // "Princess Luna",
    // "Queen Chrysalis",
    // "Rainbow Dash",
    // "Ralsei",
    // "Rarity",
    // "Rise Kujikawa",
    // "Sans",
    // "Scootaloo",
    // "Scout",
    // "Sentry Turret",
    // "Shining Armor",
    // "Silver Spoon",
    // "Snails",
    // "Sniper",
    // "Snips",
    // "Soarin'",
    // "Soldier",
    // "Sonata Dusk",
    // "Spike",
    // "Spitfire",
    // "SpongeBob SquarePants",
    // "Spy",
    // "Stanley",
    // "Starlight Glimmer",
    // "Steven Universe",
    // "Sugar Belle",
    // "Sunburst",
    // "Sunset Shimmer",
    // "Susie",
    // "Sweetie Belle",
    // "Temmie",
    // "Tenth Doctor",
    // "The Narrator",
    // "Theo",
    // "Toriel",
    // "Trixie",
    // "Twilight Sparkle",
    // "Undyne",
    // "Vapor Trail",
    // "Wheatley",
    // "Yes Man",
    // "Zecora"
]

const phrases = [
    {
        file: "hello-1.wav",
        text: "I am NOT a robot!",
    },
]

const url = "https://api.15.ai/app/getAudioFile"

const downloadVoices = async (url, body, headers, targetFile) => {
    const result = await nodeFetch({
        url,
        method: `POST`,
        headers,
        body,
        isBinary: true,
    })
    fs.writeFileSync(targetFile, result)
}

const headers = {
    'authority': 'api.15.ai',
    'access-control-allow-origin': '*',
    'accept': 'application/json, text/plain, */*',
    'user-agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 11_1_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36',
    'content-type': 'application/json;charset=UTF-8',
    'origin': 'https://15.ai',
    'sec-fetch-site': 'same-site',
    'sec-fetch-mode': 'cors',
    'sec-fetch-dest': 'empty',
    'referer': 'https://15.ai/',
    'accept-language': 'en-GB,en-US;q=0.9,en;q=0.8,no;q=0.7,sv;q=0.6',
}

const downloadFiles = async () => {

    return await Promise.mapSeries(phrases, async phrase => {
        return await Promise.mapSeries(voices, async (voice, i) => {
            const targetFolder = `/Users/robbintapper/what-are-you-carrying-sir/download-voices/voice-${i}/`
            await fs.ensureDir(targetFolder)
            const targetFile = phrase.file
            const text = phrase.text

            const params = {
                "use_diagonal": true,
                "emotions": "Contextual",
                "character": voice,
                "text": text,
            }

            await downloadVoices(url, params, headers, targetFile)
        })
    })

}

downloadFiles()
    .then(res => console.log(JSON.stringify(res, null, 2)))
    .catch(err => console.error(err))

