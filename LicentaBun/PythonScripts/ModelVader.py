from vaderSentiment.vaderSentiment import SentimentIntensityAnalyzer
import sys
import pandas as pd


def vaderSentiment(row):
    analyzer = SentimentIntensityAnalyzer()
    sentiment_scores = analyzer.polarity_scores(row)
    compound_score = sentiment_scores['compound']

    return compound_score


def main():
    if len(sys.argv) < 2:
        print("Usage: python script.py <input_file>")
        sys.exit(1)

    input_file = sys.argv[1]
    output_file = input_file.replace('.csv', '_output.csv')

    data = pd.read_csv(input_file)
    if "Sentiment" not in data.columns or "Value" not in data.columns:
        data["Sentiment"] = ""
        data["Value"] = 0.0

    # Aplicați analiza de sentiment pentru fiecare rând din coloana "Text"
    data['Sentiment'] = data['Text'].apply(vaderSentiment)

    data['Value'] = data['Sentiment']
    data['Value'] = (data['Value'] + 1) / 2
    # Actualizați valorile în coloana "Sentiment" în funcție de praguri
    data['Sentiment'] = ['positive' if sentiment > 0.1 else 'negative' if sentiment < -0.1 else 'uncertain'
                         for sentiment in data['Sentiment']]

    data.to_csv(output_file, index=False)

    print(output_file)


if __name__ == "__main__":
    main()
