import pytest

AB_CASES = [
    pytest.param((3, 1), id="edge-low", marks=pytest.mark.xfail(reason="edge case (skal fejle)")),
    pytest.param((3, 2), id="middle-pass"),
    pytest.param((3, 3), id="edge-high", marks=pytest.mark.xfail(reason="edge case (skal fejle)")),
]

@pytest.fixture(params=AB_CASES)
def ab(request):
    return request.param  # (a, b)


def test_plus_fixture(ab):
    a, b = ab
    result = a + b
    print(f"PLUS: {a} + {b} = {result}")
    assert result == 5

def test_minus_fixture(ab):
    a, b = ab
    result = a - b
    print(f"MINUS: {a} - {b} = {result}")
    assert result == 1

def test_multiply_fixture(ab):
    a, b = ab
    result = a * b
    print(f"MULT: {a} * {b} = {result}")
    assert result == 6

def test_divide_fixture(ab):
    a, b = ab
    result = a / b
    print(f"DIV: {a} / {b} = {result}")
    assert result == pytest.approx(1.5)
