// A program to invert a binary tree in Pascal
type
  // A node of a binary tree
  Node = record
    data: integer;
    left, right: ^Node;
  end;

// A function to create a new node with given data
function newNode(data: integer): ^Node;
var
  temp: ^Node;
begin
  new(temp);
  temp^.data := data;
  temp^.left := nil;
  temp^.right := nil;
  newNode := temp;
end;

// A function to invert a given binary tree recursively
function invertBinaryTree(root: ^Node): ^Node;
var
  temp: ^Node;
begin
  // Base case: if the tree is empty
  if root = nil then
    invertBinaryTree := nil
  else
  begin
    // Swap the left and right subtrees
    temp := root^.left;
    root^.left := root^.right;
    root^.right := temp;
    // Invert the left subtree
    invertBinaryTree(root^.left);
    // Invert the right subtree
    invertBinaryTree(root^.right);
    // Return the inverted root
    invertBinaryTree := root;
  end;
end;

// A procedure to print the preorder traversal of a binary tree
procedure preorder(root: ^Node);
begin
  // Base case: if the tree is empty
  if root = nil then
    exit
  else
  begin
    // Print the root data
    write(root^.data, ' ');
    // Print the left subtree
    preorder(root^.left);
    // Print the right subtree
    preorder(root^.right);
  end;
end;

// The main program
var
  root: ^Node;
begin
  // Construct the following tree
  //    1
  //   / \
  //  2   3
  // / \ / \
  //4  5 6  7
  root := newNode(1);
  root^.left := newNode(2);
  root^.right := newNode(3);
  root^.left^.left := newNode(4);
  root^.left^.right := newNode(5);
  root^.right^.left := newNode(6);
  root^.right^.right := newNode(7);

  // Print the preorder traversal of the original tree
  writeln('Preorder traversal of the original tree:');
  preorder(root);
  writeln;

  // Invert the binary tree
  root := invertBinaryTree(root);

  // Print the preorder traversal of the inverted tree
  writeln('Preorder traversal of the inverted tree:');
  preorder(root);
  writeln;
end.
