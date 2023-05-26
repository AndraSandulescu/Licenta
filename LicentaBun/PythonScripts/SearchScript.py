import sys
import datetime
import snscrape.modules.twitter as sntwitter
import pandas as pd
import json


def search(text, username, until, since, retweet, replies):
    global filename
    q = text
    if username != '':
        q += f" from:{username}"
    if until == '':
        until = datetime.datetime.strftime(datetime.date.today(), '%Y-%m-%d')
        q += f" until:{until}"
    if since == '':
        since = datetime.datetime.strftime(datetime.datetime.strptime(until, '%Y-%m-%d') - datetime.timedelta(days=7),
                                           '%Y-%m-%d')
        q += f" since:{since}"
    if retweet == 'y':
        q += f" exclude:retweets"
    if replies == 'y':
        q += f" exclude:replies"

    if username != '' and text != '':
        filename = f"{since}_{until}_{username}_{text}.csv"
    elif username != "":
        filename = f"{since}_{until}_{username}.csv"
    else:
        filename = f"{since}_{until}_{text}.csv"
    print(filename)
    return q


def scrape_tweets(text, username, since, until, retweet, replies, count):
    q = search(text, username, since, until, retweet, replies)
    tweets_list = []

    if count == -1:
        for i, tweet in enumerate(sntwitter.TwitterSearchScraper(q).get_items()):
            tweets_list.append([
                tweet.date, tweet.id, tweet.content, tweet.user.username, tweet.lang, tweet.hashtags,
                tweet.replyCount, tweet.retweetCount, tweet.likeCount, tweet.quoteCount
            ])
    else:
        i = 0
        for tweet in sntwitter.TwitterSearchScraper(q).get_items():
            if i >= count:
                break
            tweets_list.append([
                tweet.date, tweet.id, tweet.rawContent, tweet.user.username, tweet.lang, tweet.hashtags,
                tweet.replyCount, tweet.retweetCount, tweet.likeCount, tweet.quoteCount
            ])
            i += 1

    tweets_df = pd.DataFrame(
        tweets_list,
        columns=['DateTime', 'TweetId', 'Text', 'Username', 'Language', 'Hashtags', 'ReplyCount', 'RetweetCount',
                 'LikeCount', 'QuoteCount']
    )

    # Sort the DataFrame by DateTime in descending order
    tweets_df.sort_values(by='DateTime', ascending=False, inplace=True)

    # Generate a unique filename based on the query parameters
    filename = f"{since}_{until}_{username}_{text}.csv" if username != '' and text != '' else f"{since}_{until}_{username}.csv" if username != "" else f"{since}_{until}_{text}.csv"

    # Save the DataFrame to CSV file
    tweets_df.to_csv(filename, index=False)

    # Return the filename and its path as a JSON string
    response = {
        "filename": filename,
        "filepath": "D:/UPB/Licenta/LicentaBun/LicentaBun/" + filename
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
