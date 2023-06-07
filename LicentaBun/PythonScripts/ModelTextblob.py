from textblob import TextBlob
import sys
import pandas as pd


# path = "D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\Csv\\"


def sentimentModel(row):
    classifier = TextBlob(row)
    polarity = classifier.sentiment.polarity

    return polarity


def main():
    if len(sys.argv) < 2:
        print("Usage: python script.py <input_file>")
        sys.exit(1)

    input_file = sys.argv[1]
    # input_file = r'D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\Csv\\{}'.format(sys.argv[1])
    output_file = input_file.replace('.csv', '_output.csv')

    data = pd.read_csv(input_file)

    if "Sentiment" not in data.columns or "Value" not in data.columns:
        # Adăugați coloanele lipsă cu valorile implicite
        data["Sentiment"] = ""
        data["Value"] = 0.0
    # print(data.columns)

    # Aplicați analiza de sentiment pentru fiecare rând din coloana "Text"
    data['Sentiment'] = data['Text'].apply(sentimentModel)

    data['Value'] = data['Sentiment']
    data['Value'] = (data['Value'] + 1) / 2
    # Actualizați valorile în coloana "Sentiment" în funcție de praguri
    data['Sentiment'] = ['positive' if sentiment > 0.1 else 'negative' if sentiment < -0.1 else 'uncertain'
                         for sentiment in data['Sentiment']]


    data.to_csv(output_file, index=False)

    print(output_file)


if __name__ == "__main__":
    main()
