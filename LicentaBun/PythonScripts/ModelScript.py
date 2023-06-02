import sys
import pandas as pd
import torch
from torch import nn
import re
from collections import Counter
from nltk.corpus import stopwords
#import time

#start_time = time.time()
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
onehot_dict = {}

# Definirea clasei modelului SentimentRNN
class SentimentRNN(nn.Module):
    def __init__(self, no_layers, vocab_size, hidden_dim, embedding_dim, output_dim, drop_prob=0.5):
        super(SentimentRNN, self).__init__()
        self.output_dim = output_dim
        self.hidden_dim = hidden_dim
        self.no_layers = no_layers
        self.vocab_size = vocab_size

        # Embedding și straturi LSTM
        self.embedding = nn.Embedding(vocab_size, embedding_dim)
        self.lstm = nn.LSTM(input_size=embedding_dim, hidden_size=hidden_dim,
                            num_layers=no_layers, batch_first=True)

        # Strat dropout
        self.dropout = nn.Dropout(drop_prob)

        # Straturi liniare și sigmoid
        self.fc = nn.Linear(hidden_dim, output_dim)
        self.sig = nn.Sigmoid()

    def forward(self, x, hidden):
        batch_size = x.size(0)

        # Embedding și lstm_out
        embeds = self.embedding(x)
        lstm_out, hidden = self.lstm(embeds, hidden)

        # Reshape și dropout
        lstm_out = lstm_out.contiguous().view(-1, self.hidden_dim)
        out = self.dropout(lstm_out)

        # Stratul complet conectat și sigmoid
        out = self.fc(out)
        sig_out = self.sig(out)

        # Rearanjare pentru dimensiunea batch_size
        sig_out = sig_out.view(batch_size, -1)
        sig_out = sig_out[:, -1]  # Obțineți ultimul set de etichete din lot

        # Returnați ultimul output sigmoid și starea ascunsă
        return sig_out, hidden

    def init_hidden(self, batch_size):
        ''' Inițializează starea ascunsă '''
        h0 = torch.zeros((self.no_layers, batch_size, self.hidden_dim)).to(device)
        c0 = torch.zeros((self.no_layers, batch_size, self.hidden_dim)).to(device)
        hidden = (h0, c0)
        return hidden

# Funcția de preprocesare a șirului de caractere
def preprocess_string(s):
    s = re.sub(r"[^\w\s]", '', s)
    s = re.sub(r"\s+", ' ', s)
    s = re.sub(r"\d", '', s)
    return s.strip()

# Funcția de tokenizare
def tokenize(text):
    tokens = text.lower().split()
    return [onehot_dict.get(word, 0) for word in tokens if word in onehot_dict]


# Funcția de tokenizare și preprocesare a datelor de antrenament și de testare
def tokenize_data(x_train, y_train, x_test, y_test):
    word_list = []
    stop_words = set(stopwords.words('english'))
    for sent in x_train:
        for word in sent.lower().split():
            word = preprocess_string(word)
            if word not in stop_words and word != '':
                word_list.append(word)

    corpus = Counter(word_list)
    corpus_ = sorted(corpus, key=corpus.get, reverse=True)[:1000]
    onehot_dict = {w: i + 1 for i, w in enumerate(corpus_)}

    final_list_train, final_list_test = [], []
    for sent in x_train:
        final_list_train.append([onehot_dict.get(preprocess_string(word), 0) for word in sent.lower().split()
                                 if preprocess_string(word) in onehot_dict.keys()])
    for sent in x_test:
        final_list_test.append([onehot_dict.get(preprocess_string(word), 0) for word in sent.lower().split()
                                if preprocess_string(word) in onehot_dict.keys()])

    encoded_train = [1 if label == 'positive' else 0 for label in y_train]
    encoded_test = [1 if label == 'positive' else 0 for label in y_test]
    return final_list_train, encoded_train, final_list_test, encoded_test, onehot_dict


# Parametrii modelului și datele de antrenament/test
# ca in notebook antrenare
no_layers = 2
embedding_dim = 64
hidden_dim = 256
output_dim = 1
vocab_size = 1001;


# Încărcați modelul antrenat
model = SentimentRNN(no_layers, vocab_size, hidden_dim, embedding_dim, output_dim)
model.load_state_dict(torch.load('state_dict.pt', map_location=torch.device('cpu')))
model.eval()


def main():
    if len(sys.argv) < 2:
        print("Usage: python script.py <input_file>")
        sys.exit(1)

    input_file = sys.argv[1]
    #input_file = r'D:\\UPB\\Licenta\\LicentaBun\\LicentaBun\\Csv\\{}'.format(sys.argv[1])
    output_file = input_file.replace('.csv', '_output.csv')


    # Încărcați fișierul CSV
    data = pd.read_csv(input_file)
    if "Sentiment" not in data.columns or "Value" not in data.columns:
        # Adăugați coloanele lipsă cu valorile implicite
        data["Sentiment"] = ""
        data["Value"] = 0.0
    # print(data.columns)

    # Preprocesați și tokenizați textul din coloana "Text"
    x_train, y_train, x_test, y_test, vocab = tokenize_data(data['Text'], data['Sentiment'], [], [])

    # Transformați tokenizările în tensori pentru a le trece prin model
    inputs = [torch.tensor(tokenized_text) for tokenized_text in x_train]

    # Creeați tensorul de dimensiuni (batch_size, sequence_length) pentru inputul modelului
    max_seq_length = max(len(seq) for seq in inputs)
    padded_inputs = torch.zeros((len(inputs), max_seq_length), dtype=torch.long)
    for i, seq in enumerate(inputs):
        padded_inputs[i, :len(seq)] = seq

    # Treceti tensorul prin model pentru a obține sentimentul
    with torch.no_grad():
        outputs, _ = model(padded_inputs, model.init_hidden(len(inputs)))

    # Adăugați rezultatele (sentimentul) în coloana "Sentiment" a DataFrame-ului
    # data['Sentiment'] = ['positive' if output > 0.5 else 'negative' for output in outputs]
    data['Sentiment'] = ['positive' if output > 0.45 else 'negative' if output < 0.4 else 'uncertain' for output in
                         outputs]

    # Add the numerical value from the output to the "Value" column
    data['Value'] = outputs.tolist()

    # Salvați DataFrame-ul actualizat în același fișier CSV
    data.to_csv(output_file, index=False)

    print(output_file)

if __name__ == "__main__":
    main()


#end_time = time.time()
#elapsed_time = end_time - start_time
## print("Elapsed time: {:.2f} seconds".format(elapsed_time))

## statistica
#positive_count = len(data[data['Sentiment'] == 'positive'])
#negative_count = len(data[data['Sentiment'] == 'negative'])
#total_entries = len(data)
#positive_percentage = (positive_count / total_entries) * 100
#negative_percentage = (negative_count / total_entries) * 100

# Print the percentages
# print("Positive percentage: {:.2f}%".format(positive_percentage))
# print("Negative percentage: {:.2f}%".format(negative_percentage))
