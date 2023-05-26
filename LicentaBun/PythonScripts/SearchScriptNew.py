import sys
import datetime
import snscrape.modules.twitter as sntwitter
import pandas as pd
import json
from nltk.corpus import wordnet
from itertools import product

# Funcție pentru a obține sinonimele unui cuvânt
def get_synonyms(word):
    synonyms = set()
    for syn in wordnet.synsets(word):
        for lemma in syn.lemmas():
            synonyms.add(lemma.name())
    return synonyms

def search(text, username, until, since, retweet, replies):
    global filename
    if username != '' and text != '':
        filename = f"{since}_{until}_{username}_{text}.csv"
    elif username != "":
        filename = f"{since}_{until}_{username}.csv"
    else:
        filename = f"{since}_{until}_{text}.csv"

    q = ""
    words = text.split()

    if len(words) > 0:
        # Obține sinonimele pentru fiecare cuvânt din text
        synonyms_lists = [list(get_synonyms(word)) for word in words]
        synonym_combinations = product(*synonyms_lists)

        combinations_text = [' '.join(combination) for combination in synonym_combinations]
        q = ' OR '.join(combinations_text)

    if username != '':
        q += f" from:{username}"
    if until == '':
        until = datetime.datetime.strftime(datetime.date.today(), '%Y-%m-%d')
        q += f" until:{until}"
    if since == '':
        since = datetime.datetime.strftime(datetime.datetime.strptime(until, '%Y-%m-%d') - datetime.timedelta(days=7), '%Y-%m-%d')
        q += f" since:{since}"
    if retweet == 'y':
        q += f" exclude:retweets"
    if replies == 'y':
        q += f" exclude:replies"

    print(filename)
    return q


def scrape_tweets(text, username, since, until, retweet, replies, count):
    q = search(text, username, since, until, retweet, replies)
    tweets_list = []

    # Obține sinonimele pentru fiecare cuvânt din text
    words = text.split()
    synonyms_lists = [list(get_synonyms(word)) for word in words]
    synonym_combinations = product(*synonyms_lists)

    if count == -1:
        for i, tweet in enumerate(sntwitter.TwitterSearchScraper(q).get_items()):
            for combination in synonym_combinations:
                if all(word in tweet.content for word in combination):
                    tweets_list.append([
                        tweet.date, tweet.id, tweet.content, tweet.user.username, tweet.lang, tweet.hashtags,
                        tweet.replyCount, tweet.retweetCount, tweet.likeCount, tweet.quoteCount
                    ])
                    break
    else:
        i = 0
        for tweet in sntwitter.TwitterSearchScraper(q).get_items():
            if i >= count:
                break
            for combination in synonym_combinations:
                if all(word in tweet.content for word in combination):
                    tweets_list.append([
                        tweet.date, tweet.id, tweet.rawContent, tweet.user.username, tweet.lang, tweet.hashtags,
                        tweet.replyCount, tweet.retweetCount, tweet.likeCount, tweet.quoteCount
                    ])
                    break
            i += 1

    # Elimina duplicatele
    unique_tweets_list = [list(t) for t in set(tuple(tweet) for tweet in tweets_list)]

    tweets_df = pd.DataFrame(
        unique_tweets_list,
        columns=['DateTime', 'TweetId', 'Text', 'Username', 'Language', 'Hashtags', 'ReplyCount', 'RetweetCount',
                 'LikeCount', 'QuoteCount']
    )

    # Sortează DataFrame-ul după DateTime în ordine descendentă
    tweets_df.sort_values(by='DateTime', ascending=False, inplace=True)

    # Generează un nume de fișier unic pe baza parametrilor interogării
    filename = f"{since}_{until}_{username}_{text}.csv" if username != '' and text != '' else f"{since}_{until}_{username}.csv" if username != "" else f"{since}_{until}_{text}.csv"

    # Salvează DataFrame-ul în fișier CSV
    tweets_df.to_csv(filename, index=False)

    # Returnează numele de fișier și calea acestuia sub formă de string JSON
    response = {
        "filename": filename,
        "filepath": "/path/to/your/directory/" + filename
    }
    return json.dumps(response)


if __name__ == "__main__":
    if len(sys.argv) < 8:
        print("Usage: python SearchScript.py <text> <username> <since> <until> <retweet> <replies> <count>")
        sys.exit(1)

    text = sys.argv[1]
    username = sys.argv[2]
    since = sys.argv[3]
    until = sys.argv[4]
    retweet = sys.argv[5]
    replies = sys.argv[6]
    count = int(sys.argv[7])

    result = scrape_tweets(text, username, since, until, retweet, replies, count)
    print(result)
