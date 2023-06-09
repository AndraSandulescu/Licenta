import datetime
import snscrape.modules.twitter as sntwitter
import pandas as pd
import json
import os

path = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\NewsCsv\\"
existing_file = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\NewsCsv\\news.csv"


def search(text, username, since, until, retweet, replies):
    global filename

    q = text  # query-ul de căutare
    if username:
        q += f" (from:{username})"

    if until == '':
        until = datetime.datetime.strftime(datetime.date.today(), '%Y-%m-%d')
    q += f" until:{until}"

    if since == '':
        since = datetime.datetime.strftime(datetime.datetime.strptime(until, '%Y-%m-%d') - datetime.timedelta(days=7),
                                           '%Y-%m-%d')
    q += f" since:{since}"

    if retweet == 'n':
        q += f" exclude:retweets"
    if replies == 'n':
        q += f" exclude:replies"

    filename = f"{username}_{since}_{until}.csv"
    print(filename)
    print("query:")
    print(q)
    return q


def scrape_tweets(text, username, since, until, retweet, replies):
    q = search(text, username, since, until, retweet, replies)
    tweets_list = []

    for tweet in sntwitter.TwitterSearchScraper(q).get_items():
        tweets_list.append([
            tweet.date, tweet.id, tweet.rawContent, tweet.user.username, tweet.user.displayname, tweet.lang,
            tweet.hashtags,
            tweet.replyCount, tweet.retweetCount, tweet.likeCount, tweet.quoteCount
        ])

    tweets_df = pd.DataFrame(
        tweets_list,
        columns=['DateTime', 'TweetId', 'Text', 'Username', 'displayname', 'Language', 'Hashtags', 'ReplyCount',
                 'RetweetCount',
                 'LikeCount', 'QuoteCount']
    )

    # Sortează DataFrame-ul după DateTime în ordine descrescătoare
    tweets_df.sort_values(by='DateTime', ascending=False, inplace=True)

    # Calea către fișierul generat acum
    output_file = os.path.join(path, filename)

    # Salvare DataFrame în CSV
    tweets_df.to_csv(output_file, index=False)
    print("\ngata 1 sortat\n")
    tweets_df.drop_duplicates(subset=['Text'], inplace=True)

    # Concatenare fișier generat acum cu fișierul existent, eliminând duplicatelor
    if os.path.exists(existing_file):
        existing_df = pd.read_csv(existing_file)
        combined_df = pd.concat([tweets_df, existing_df], ignore_index=True)

        # Elimină duplicatelor din DataFrame-ul combinat
        combined_df.drop_duplicates(subset=['Text'], inplace=True)

        # Convertirea coloanei 'DateTime' în formatul corect
        combined_df['DateTime'] = pd.to_datetime(combined_df['DateTime'])

        # Sortarea DataFrame-ului combinat după 'DateTime'
        combined_df.sort_values(by='DateTime', ascending=False, inplace=True)

        # Salvarea DataFrame-ului combinat în CSV
        combined_df.to_csv(existing_file, index=False)

        # Verifică dacă numărul de înregistrări depășește 1500
        if len(combined_df) > 1500:
            combined_df = combined_df.head(1500)
            combined_df.to_csv(existing_file, index=False)
    else:
        # Salvare DataFrame în CSV dacă fișierul existent nu există
        tweets_df.to_csv(existing_file, index=False)


if __name__ == "__main__":
    text = ""  # Textul de căutare
    usernames = ["AOC","BillClinton","BarackObama","SenSchumer", "CoryBooker", "ewarren", "HillaryClinton",  "IlhanMN", "JoeBiden", "KamalaHarris", 
                 "LindseyGrahamSC", "marcorubio",  "MichelleObama", "LeaderMcConnell", "SpeakerPelosi", "RandPaul", "tedcruz"]  # Lista statică de utilizatori
    until = datetime.datetime.now().strftime('%Y-%m-%d')  # Data până la care să se facă căutarea (astăzi)
    since = (datetime.datetime.now() - datetime.timedelta(hours=720)).strftime(
        '%Y-%m-%d')  # Data de la care să se facă căutarea (ultimele 24 de ore)
    retweet = "y"  # Include retweet-uri (da)
    replies = "y"  # Include răspunsuri (da)

    for username in usernames:
        result = scrape_tweets(text, username, since, until, retweet, replies)
        #print(result)
